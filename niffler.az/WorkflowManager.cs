using Alpaca.Markets;
using Microsoft.Extensions.Logging;
using Niffler.Data;
using Niffler.Data.ATS;
using Niffler.Interfaces;
using Niffler.Modules;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Niffler
{
	/// <summary>
	/// Determines which module is in operation for the job, instantiates the DispatchRoundup module,
	/// and runs the job from within DispatchRoundup.
	/// Only called by TradeOperationProcessor.
	/// </summary>
	public class WorkflowManager : WorkflowManagerBase, IWorkflowManager
	{
		public WorkflowManager(ILogger log) : base(log)
		{
		}

		public ServiceQueueDTO.TaskDTO Job { get; private set; }

		public async Task RunAsync<T>(T dto)
		{
			Job = dto as ServiceQueueDTO.TaskDTO;
			bool isWorkDone = false;
			IModule module = null;
			double equity = 0;
			if (!Cache.ServiceQueues.Exists(x => x.ServiceQueueID == Job.ServiceQueueID))
			{
				using (var ops = new ServiceQueueOps())
				{
					var queueItem = new ServiceQueue(Job);
					Cache.ServiceQueues.Add(queueItem);
					await ops.Merge(queueItem);
				}
			}
			switch (Job.QueueItem.ModuleType)
			{
				case ModuleType.EasyInOut:
				case ModuleType.Niffler:
					module = new DispatchRoundup(new ServiceManager(Logger));
					isWorkDone = await module.RunAsync(Job);
					break;
			}
			await Cache.AccountUpdateSemaphore.WaitAsync();
			try
			{
				using (var ops = new AccountOps())
				{
					Account account = Cache.Accounts.Find(x => x.AccountID == Job.AccountID);
					if (account.BrokerageAccountID != "0" && !String.IsNullOrWhiteSpace(account.BrokerageAccountID))
					{
						IAccount accountAPCA = await Cache.TradingClient(account.AccountID).GetAccountAsync();
						equity = (double)accountAPCA.Equity;
					}
					else
					{
						equity = Cache.Workflows.FindAll(x => x.AccountID == Job.AccountID).Select(x => x.CurrentBalance).Sum();
					}
					if (account.CurrentBalance != equity)
					{
						double balance = account.CurrentBalance.Value;
						double pl = Math.Round(equity - balance, 4);
						double plpct = Math.Round((pl / balance) * 100, 2);
						NotificationDTO alert = new NotificationDTO()
						{
							AccountName = account.Description,
							AlertType = AlertType.DailyPerformance,
							FromAddress = Cache.NotifyFromAddress,
							ToAddress = account.NotifyRecipient,
							PL = pl,
							PLPct = plpct
						};
						account.CurrentBalance = equity;
						account.LastUpdated = Util.CurrentDateTimeEST;
						await ops.Merge(account);
						if (account.Notify) { await Util.RetryOnExceptionAsync(async () => await Store.PushMessageToQueue(Cache.NotificationMonitorQueueName, Store.Serialize(alert)), this.Logger); };
					}
				}
			}
			finally
			{
				Cache.AccountUpdateSemaphore.Release();
			}
		}
	}
}