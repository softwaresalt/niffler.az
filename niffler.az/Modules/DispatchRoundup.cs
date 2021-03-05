using Alpaca.Markets;
using Microsoft.Extensions.Logging;
using Niffler.Data;
using Niffler.Data.ATS;
using Niffler.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Niffler.Modules
{
	/// <summary>
	/// Common Module: Responsible for session start-up/shut-down operations,
	/// and workflow operation and job/task dispatching.
	/// Jobs/tasks have multiple operations; this module handles only the roundup
	/// at start & end of day.
	/// Only called by WorkflowManager.
	/// </summary>
	public class DispatchRoundup : Module<IServiceManager>, IModule
	{
		public DispatchRoundup(IServiceManager manager) : base(manager)
		{
		}

		public ServiceQueueDTO.TaskDTO task { get; private set; }
		public TimeSpan Delay { get; set; } = new TimeSpan(0, 1, 0);
		public object DTO { get; set; }

		public async Task<bool> RunAsync<T>(T entity)
		{
			//This module only uses ServiceQueueDTO.TaskDTO
			task = entity as ServiceQueueDTO.TaskDTO;
			bool isAllWorkDone = false, isWorkDone, pushDelayedMessage = false, isOutsideOperatingWindow = false;
			long accountID = Cache.Workflows.Find(x => x.WorkflowID == task.WorkflowID).AccountID;
			Account account = Cache.Accounts.Find(x => x.AccountID == accountID);
			Workflow workflow = Cache.Workflows.Find(x => x.WorkflowID == task.WorkflowID);
			task.QueueItem.OperationQueue = new Queue<ServiceQueueDTO.Operation>(task.QueueItem.OperationQueue.OrderBy(x => x.Ordinal));
			while (task.QueueItem.OperationQueue != null && task.QueueItem.OperationQueue.Count > 0)
			{
				if (task.QueueTime == null) { task.QueueTime = Util.CurrentEST; }
				if (task.QueueDate.Date != Util.CurrentDateEST.Date) { isOutsideOperatingWindow = true; break; }
				ServiceQueueDTO.Operation operation = task.QueueItem.OperationQueue.Peek();
				if (task.StartTimeEST != operation.StartTimeEST) { task.StartTimeEST = operation.StartTimeEST; }
				if (task.RunByTimeEST != operation.RunByTimeEST) { task.RunByTimeEST = operation.RunByTimeEST; }
				if (task.StartTimeEST > Util.CurrentEST) { pushDelayedMessage = true; break; }
				List<ScreenLog> screenLogs = new List<ScreenLog>();
				List<DispatchSymbol> newSymbols = new List<DispatchSymbol>();
				List<DispatchSymbol> dispatchSymbols = new List<DispatchSymbol>();
				isWorkDone = false;
				switch (operation.OperationType)
				{
					case OperationType.FetchScreenData:
						screenLogs = await Manager.RunService<ScreenLog>(operation, task.WorkflowID, task.QueueDate.Date, task.ServiceQueueID, null);
						if (screenLogs.Count < operation.Minimum.Value)
						{
							Manager.Logger.LogWarning($"Screen log minimum of {operation.Minimum} was not met; screen service produced only {screenLogs.Count} results.");
							break;
						}
						screenLogs = await viableSymbols(accountID, screenLogs.OrderBy(x => x.Ordinal).ToList(), operation.Limit);
						using (ScreenLogOps ops = new ScreenLogOps())
						{
							await Util.RetryOnExceptionAsync(async () => await ops.MergeBatch(screenLogs), Manager.Logger);
							Cache.ScreenLogs.AddRange(screenLogs);
							dispatchSymbols = Cache.DispatchSymbols.Where(x => x.PartitionKey == Util.ConformedKey(task.QueueDate.Date, task.AccountID.ToString())).ToList();
							foreach (ScreenLog sl in screenLogs)
							{
								if (!dispatchSymbols.Exists(x => x.RowKey == sl.Symbol))
								{
									var symbol = new DispatchSymbol(Util.ConformedKey(task.QueueDate.Date, task.AccountID.ToString()), sl.Symbol, sl.Shortable);
									newSymbols.Add(symbol);
								}
							}
						}
						newSymbols = await dispatchTrades(newSymbols, account, task.QueueItem.ModuleType);
						Cache.DispatchSymbols.AddRange(newSymbols);
						using (DispatchSymbolOps ops = new DispatchSymbolOps())
						{
							await Util.RetryOnExceptionAsync(async () => await ops.MergeBatch(newSymbols), Manager.Logger);
						}
						isWorkDone = true;
						break;

					case OperationType.MarketSell:
						isWorkDone = true; //Just need to allow this to complete and let WorkflowManager tally up day's account P/L
						Cache.TickerInProcess.Clear();
						break;
				}

				if (operation.OperationType == OperationType.FetchScreenData && screenLogs.Count < (int)operation.Minimum)
				{ //Wait 1min; try screen again; avoid function shut-down between attempts (Azure 5 min inactivity exit)
					Delay = TimeSpan.FromMinutes(1);
					pushDelayedMessage = true;
					isWorkDone = false;
					break; //exit out of loop so task/operation can be pushed back into storage queue
				}
				if (isWorkDone)
				{
					task.QueueItem.OperationQueue.Dequeue(); //dequeue here to ensure proper peek + operational integrity if task was completed
					task.CompletionDate = Util.CurrentDateTimeEST;
					isAllWorkDone = (task.QueueItem.OperationQueue.Count == 0);
				}
				else
				{
					Delay = TimeSpan.FromMinutes(1);
					pushDelayedMessage = true;
					break; //exit out of loop so task/operation can be pushed back into storage queue
				}
				using (ServiceQueueOps ops = new ServiceQueueOps())
				{
					var queueItem = Cache.ServiceQueues.Find(x => x.ServiceQueueID == task.ServiceQueueID);
					queueItem.QueueItem = task.SerializeQueueItem();
					queueItem.StartTimeEST = Util.ConformedTimeString(task.StartTimeEST);
					queueItem.RunByTimeEST = Util.ConformedTimeString(task.RunByTimeEST);
					if (task.CompletionDate != null) { queueItem.CompletionDate = task.CompletionDate; }
					await Util.RetryOnExceptionAsync(async () => await ops.Merge(queueItem), Manager.Logger);
				}
			}
			if (task.QueueDate.Date == Util.CurrentDateEST && task.CompletionDate == null && pushDelayedMessage)
			{
				DateTime startTime = task.QueueDate.Date.Add(task.StartTimeEST);
				Delay = (startTime - Util.CurrentDateTimeEST) < Delay ? Delay : (startTime - Util.CurrentDateTimeEST);
			}
			if (pushDelayedMessage) { await Util.RetryOnExceptionAsync(async () => await Store.PushMessageToQueue(Cache.TradeOperationQueueName, Store.Serialize(new JobDTO(task)), Delay), Manager.Logger); }
			if (isOutsideOperatingWindow)
			{
				Manager.Logger.LogWarning($"For queue entry ({task.ServiceQueueID}), current run time ({Util.CurrentDateTimeEST}) is outside the task operating window ({task.QueueDate.Add(task.StartTimeEST)}) to ({task.QueueDate.Add(task.RunByTimeEST)})." +
					Environment.NewLine + "Queue entry will not be requeued.");
			}

			return isAllWorkDone;
		}

		/// <summary>
		/// Lookup current trading session hours; create trading jobs to track throughout session;
		/// dispatch jobs to queue for session operations.
		/// </summary>
		/// <param name="dispatchSymbols">Set of symbols picked from screen</param>
		/// <param name="account">Account object; includes account ID.</param>
		/// <param name="module">Type of module to run.</param>
		/// <returns>Returns list of dispatched symbols queued for operations.</returns>
		private async Task<List<DispatchSymbol>> dispatchTrades(List<DispatchSymbol> dispatchSymbols, Account account, ModuleType module)
		{
			var tradingClient = Cache.TradingClient(account.AccountID);
			IAccount accountAPCA = await Util.RetryOnExceptionWithReturnAsync(async () => await tradingClient.GetAccountAsync(), Manager.Logger);
			double splitAmount = (double)accountAPCA.TradableCash / dispatchSymbols.Count;
			int orderCount = Cache.DispatchSymbols.Count();
			List<DispatchSymbol> dispatchedSymbols = new List<DispatchSymbol>();
			CalendarRequest cr = new CalendarRequest();
			cr.SetInclusiveTimeInterval(task.QueueDate.AddDays(-7), task.QueueDate);
			var iCal = await tradingClient.ListCalendarAsync(cr);
			List<ICalendar> calendar = iCal.OrderBy(x => x.TradingDateUtc).ToList();
			int index = calendar.FindIndex(x => x.TradingDateUtc == task.QueueDate);
			if (index < 1)
			{
				return dispatchedSymbols;
			}
			ICalendar curMarketDaySchedule = calendar[index];
			ICalendar prevMarketDaySchedule = calendar[index - 1];
			foreach (DispatchSymbol ds in dispatchSymbols)
			{
				TradeDispatchDTO dto = new TradeDispatchDTO(task, module)
				{
					AccountName = account.Description,
					Symbol = ds.RowKey,
					IsShortable = ds.Shortable,
					TradeBalance = splitAmount,
					StartCurrentSessionUTC = curMarketDaySchedule.TradingOpenTimeUtc,
					EndCurrentSessionUTC = curMarketDaySchedule.TradingCloseTimeUtc,
					EndPreviousSessionUTC = prevMarketDaySchedule.TradingCloseTimeUtc.AddHours(-2)
				};
				try
				{
					await Util.RetryOnExceptionAsync(async () => await Store.PushMessageToQueue(Cache.TradeDispatchMonitorQueueName, Store.Serialize(dto)), Manager.Logger);
					dispatchedSymbols.Add(ds);
				}
				catch { }
			}
			return dispatchedSymbols;
		}

		/// <summary>
		/// Checks symbol against list of assets covered by Alpaca API
		/// </summary>
		/// <param name="accountID">ID of Alpaca client to fetch from cache</param>
		/// <param name="screenLogs">Full list of screen objects.</param>
		/// <param name="topN">The count of symbols to check from screen</param>
		/// <returns>List of top(n) viable screen objects.</returns>
		private async Task<List<ScreenLog>> viableSymbols(long accountID, List<ScreenLog> screenLogs, int topN)
		{
			int limitCount = 0;
			var tradingClient = Cache.TradingClient(accountID);
			List<ScreenLog> viableScreen = new List<ScreenLog>();
			foreach (ScreenLog sl in screenLogs)
			{
				if (limitCount == topN) { break; }
				try
				{
					IAsset asset = await tradingClient.GetAssetAsync(sl.Symbol);
					if (asset.IsTradable && asset.Marginable && asset.Status == AssetStatus.Active)
					{
						sl.Shortable = (asset.Shortable && asset.EasyToBorrow);
						viableScreen.Add(sl);
						limitCount++;
					}
				}
				catch (RestClientErrorException e)
				{
					//If asset doesn't exist on server, rest client throws 404: continue on to next symbol
					if (e.ErrorCode == 40410000) { continue; } else { throw; }
				}
				catch
				{
					throw;
				}
			}
			return viableScreen;
		}
	}
}