using Niffler.Data;
using Niffler.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using c = Niffler.Modules.APCA.Common;

namespace Niffler.Modules
{
	/// <summary>
	/// Demo Module:
	/// Example of recurring trades throughout session based on indicator signals.
	/// Called by TradeDispatchManager; responsible for performing actual trades
	/// and determining when to buy/sell.
	/// </summary>
	public class NifflerMod : Module<IServiceManager>, IModule
	{
		public NifflerMod(IServiceManager manager) : base(manager)
		{
		}

		public TimeSpan Delay { get; set; } = TimeSpan.FromSeconds(30);
		public TradeDispatchDTO Job { get; private set; }
		public object DTO { get; set; }

		public async Task<bool> RunAsync<T>(T entity)
		{
			Job = entity as TradeDispatchDTO;
			await Util.RetryOnExceptionAsync(async () => await Cache.LoadTradeBookData(Job.WorkflowID, Job.TradeDate.Date), Manager.Logger);
			if (!Job.InPosition && Util.CurrentEST > Cache.TimeBoundary[Boundary.T1530]) { return true; } //All work is done for the day.
			bool isAllWorkDone = false, isTradeComplete = false;
			if (Job.OrderID != Guid.Empty)
			{
				int ledgerSign = (Job.InPosition) ? -1 : 1;
				var tradeBook = Cache.TradeBooks.Find(x => x.ServiceQueueID == Job.ServiceQueueID && x.OrderID == Job.OrderID);
				isTradeComplete = await c.GetOrderStatus(Job, tradeBook, ledgerSign, Job.InPosition, Manager.Logger);
				if (isTradeComplete) { await c.SyncTradebook(tradeBook, Manager.Logger); } else { return false; }
			}
			DateTime tradeDateStart = Util.ConvertUTCtoEST(Job.StartCurrentSessionUTC);
			List<Quote> quotes = await c.FetchValidateQuotes(Job, tradeDateStart, Manager.Logger);
			EventData quantData = new EventData(quotes, Job.Symbol, tradeDateStart, Job.TradeBalance, Job);
			NifflerQuant quant = new NifflerQuant(quantData);
			if (Job.InPosition) { if (await c.CheckPositionManualClose(Job, quant.QMonitor.Data, Manager.Logger)) { return false; }; }
			Trade trade = quant.ComputeTrades();
			if (trade != null)
			{
				switch (trade.State)
				{
					case TradeState.Open:
						isTradeComplete = await c.MarketBuy(Job, trade, Manager.Logger);
						break;

					case TradeState.Close:
						isTradeComplete = await c.MarketSell(Job, trade, Manager.Logger);
						break;

					case TradeState.MarketEoD:
						isAllWorkDone = true;
						break;
				}
			}
			DTO = Job;
			return isAllWorkDone;
		}
	}
}