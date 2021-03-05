using Alpaca.Markets;
using Niffler.Data;
using Niffler.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using c = Niffler.Modules.APCA.Common;

namespace Niffler.Modules
{
	/// <summary>
	/// Demo Module
	/// Take the top 3 screen picks at 10:00 EST and let ride until 13:30 EST unless 2.5% stop-loss triggered
	/// </summary>
	public class EasyInOut : Module<IServiceManager>, IModule
	{
		public EasyInOut(IServiceManager manager) : base(manager)
		{
		}

		public TimeSpan Delay { get; set; } = TimeSpan.FromSeconds(60);
		public TradeDispatchDTO Job { get; private set; }
		public object DTO { get; set; }

		public async Task<bool> RunAsync<T>(T entity)
		{
			Job = entity as TradeDispatchDTO;
			await Util.RetryOnExceptionAsync(async () => await Cache.LoadTradeBookData(Job.WorkflowID, Job.TradeDate.Date), Manager.Logger);
			if (!Job.InPosition && Util.CurrentEST > Cache.TimeBoundary[Boundary.T1530]) { return true; } //All work is done for the day.
			bool isAllWorkDone = false, isTradeComplete = false;
			Trade trade = null;
			if (Job.OrderID != Guid.Empty)
			{
				int ledgerSign = (Job.InPosition) ? -1 : 1;
				var tradeBook = Cache.TradeBooks.Find(x => x.ServiceQueueID == Job.ServiceQueueID && x.OrderID == Job.OrderID);
				isTradeComplete = await c.GetOrderStatus(Job, tradeBook, ledgerSign, Job.InPosition, Manager.Logger);
				if (isTradeComplete) { await c.SyncTradebook(tradeBook, Manager.Logger); } else { return false; }
			}
			DateTime tradeDateStart = Util.ConvertUTCtoEST(Job.StartCurrentSessionUTC);

			if (!Job.InPosition && Util.CurrentEST > Cache.TimeBoundary[Boundary.T1030]) { return true; }
			if (!Job.InPosition && Util.CurrentEST >= Cache.TimeBoundary[Boundary.T1000])
			{
				List<Quote> quotes = await c.FetchValidateQuotes(Job, tradeDateStart, Manager.Logger);
				EventData quantData = new EventData(quotes, Job.Symbol, tradeDateStart, Job.TradeBalance, Job);
				quantData.LoadSourceData();
				quantData.FillQuants(4, 6, 5);
				int ticks = quantData.IntraDayQuotes.Count - 1;
				trade = c.RegisterBuyPosition(quantData, TradeRuleGroup.None, BuySellCase.None, ticks);
			}
			else
			if (Job.InPosition)
			{
				List<Quote> quotes = await c.FetchValidateQuotes(Job, tradeDateStart, Manager.Logger);
				EventData quantData = new EventData(quotes, Job.Symbol, tradeDateStart, Job.TradeBalance, Job);
				quantData.LoadSourceData();
				quantData.FillQuants(4, 6, 5);
				int ticks = quantData.IntraDayQuotes.Count - 1;
				if (await c.CheckPositionManualClose(Job, quantData, Manager.Logger)) { return true; };
				IPosition position = await c.GetPositionAsync(Job, Manager.Logger);
				if (position != null && (position.IntradayUnrealizedProfitLossPercent <= -.02M || Util.CurrentEST >= Cache.TimeBoundary[Boundary.T1330]))
				{
					trade = c.RegisterSellPosition(quantData, ticks);
				}
			}

			if (trade != null)
			{
				switch (trade.State)
				{
					case TradeState.Open:
						isTradeComplete = await c.MarketBuy(Job, trade, Manager.Logger);
						break;

					case TradeState.Close:
						isAllWorkDone = isTradeComplete = await c.MarketSell(Job, trade, Manager.Logger);
						break;
				}
			}
			DTO = Job;
			return isAllWorkDone;
		}
	}
}