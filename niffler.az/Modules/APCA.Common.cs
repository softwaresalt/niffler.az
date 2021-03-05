using Alpaca.Markets;
using Microsoft.Extensions.Logging;
using Niffler.Data;
using Niffler.Data.ATS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Niffler.Modules.APCA
{
	/// <summary>
	/// Encapsulates all common functions for interacting with the Alpaca API
	/// To Do: Create similar adapters for Interactive Brokers, TD Ameritrade, E-Trade, others?
	/// </summary>
	public static class Common
	{
		public static OrderStatus[] OrderStatusFill = new OrderStatus[] { OrderStatus.Fill, OrderStatus.Filled };

		public static OrderStatus[] OrderStatusEnd = new OrderStatus[] {
				OrderStatus.Canceled, OrderStatus.DoneForDay, OrderStatus.Expired, OrderStatus.PendingCancel,
				OrderStatus.Rejected, OrderStatus.PendingReplace, OrderStatus.Replaced, OrderStatus.Stopped, OrderStatus.Suspended };

		public static async Task SyncTradebook(this TradeBook tradeBook, ILogger logger)
		{
			using (TradeBookOps ops = new TradeBookOps())
			{
				await Util.RetryOnExceptionAsync(async () => await ops.Merge(tradeBook), logger);
				Cache.MergeTradeIntoBook(tradeBook);
			}
		}

		public static async Task<IPosition> GetPositionAsync(this TradeDispatchDTO job, ILogger logger)
		{
			var tradingClient = Cache.TradingClient(job.AccountID);
			IReadOnlyList<IPosition> positions = await Util.RetryOnExceptionWithReturnAsync(async () => await tradingClient.ListPositionsAsync(), logger);
			return positions.Where(x => x.Symbol == job.Symbol).FirstOrDefault();
		}

		public static async Task<bool> CheckPositionManualClose(this TradeDispatchDTO job, EventData quantData, ILogger logger)
		{
			bool isManualClosePerformed = false;
			Account account = Cache.Accounts.Find(x => x.AccountID == job.AccountID);
			var tradingClient = Cache.TradingClient(job.AccountID);
			//IReadOnlyList<IPosition> positions = await Util.RetryOnExceptionWithReturnAsync(async () => await tradingClient.ListPositionsAsync(), logger);
			IPosition position = await GetPositionAsync(job, logger);
			if (position == null) { return isManualClosePerformed; }
			List<TradeBook> tradeCache = Cache.TradeBooks.Where(x => x.ServiceQueueID == job.ServiceQueueID).ToList();
			ListOrdersRequest lor = new ListOrdersRequest() { OrderStatusFilter = OrderStatusFilter.Closed, OrderListSorting = SortDirection.Ascending };
			lor.SetExclusiveTimeInterval(Util.MarketOpenUTC, Util.MarketClosePositionsByUTC);
			var ordersRO = await Util.RetryOnExceptionWithReturnAsync(async () => await tradingClient.ListOrdersAsync(lor), logger);
			List<IOrder> ordersClosed = ordersRO.ToList();
			ordersClosed.RemoveAll(x => !OrderStatusFill.Contains(x.OrderStatus) || x.OrderSide == OrderSide.Buy); //Exclude non-filled orders
			tradeCache.ForEach(x => ordersClosed.RemoveAll(a => a.OrderId == x.OrderID && x.Amount.HasValue && x.Amount.Value != 0)); //exclude all known/recorded orders that have recorded price/amount.
			foreach (IOrder closedOrder in ordersClosed)
			{
				if (closedOrder.Symbol == job.Symbol)
				{
					TradeBook tradeBook = new TradeBook(Util.ConformedKey(job.WorkflowID.ToString(), job.TradeDate.Date))
					{
						Quantity = (int)closedOrder.Quantity,
						LastQuotePrice = quantData.LastQuotePrice,
						LastQuoteVolume = quantData.LastQuoteVolume,
						CurrentVolume = quantData.CurrentVolume,
						CurrentOBV = quantData.CurrentOBV3,
						ServiceQueueID = job.ServiceQueueID,
						Symbol = closedOrder.Symbol,
						AvgFillPrice = (double)closedOrder.AverageFillPrice,
						FillDateTime = Util.ConvertUTCtoEST(closedOrder.FilledAtUtc.Value),
						Amount = Math.Round((double)closedOrder.AverageFillPrice * closedOrder.Quantity, 4),
						ePositionType = (closedOrder.OrderSide == OrderSide.Sell) ? PositionType.ManualClose : PositionType.Opened,
						WorkflowID = job.WorkflowID,
						OrderID = closedOrder.OrderId
					};
					if (tradeCache.Exists(x => x.OrderID == closedOrder.OrderId))
					{
						tradeBook.RowKey = tradeCache.First(x => x.OrderID == closedOrder.OrderId).RowKey;
					}
					else
					{
						tradeBook.SetRowKey();
					}
					job.OrderID = Guid.Empty;
					job.PositionType = PositionType.None;
					job.PL = Math.Round(tradeBook.Amount.Value - job.PurchaseBalance, 4);
					tradeBook.ProfitLoss = job.PL;
					job.PurchaseBalance = 0D;
					job.TradeBalance += tradeBook.Amount.Value;
					job.InPosition = false;
					job.LastSellMinute = tradeBook.FillDateTime.Value.TimeOfDay;
					job.LastBuyMinute = TimeSpan.Zero;
					job.AvgFillPrice = tradeBook.AvgFillPrice;
					job.FillDate = tradeBook.FillDateTime.Value;
					NotificationDTO alert = new NotificationDTO(job, Cache.NotifyFromAddress, account.NotifyRecipient);
					job.Quantity = 0;
					isManualClosePerformed = true;
					await SyncTradebook(tradeBook, logger);
					if (account.Notify) { await Util.RetryOnExceptionAsync(async () => await Store.PushMessageToQueue(Cache.NotificationMonitorQueueName, Store.Serialize(alert)), logger); };
				}
			}
			return isManualClosePerformed;
		}

		public static async Task<bool> GetOrderStatus(this TradeDispatchDTO job, TradeBook tradeBook, int ledgerSign, bool inPosition, ILogger logger)
		{
			bool isTradeDone = false;
			Account account = Cache.Accounts.Find(x => x.AccountID == job.AccountID);
			var tradingClient = Cache.TradingClient(job.AccountID);
			for (int i = 0; i < 5; i++)
			{
				await Task.Delay(TimeSpan.FromSeconds(2));
				IOrder order = await Util.RetryOnExceptionWithReturnAsync(async () => await tradingClient.GetOrderAsync(tradeBook.OrderID), logger);
				if (OrderStatusFill.Contains(order.OrderStatus))
				{
					tradeBook.Quantity = (int)order.Quantity;
					tradeBook.AvgFillPrice = (double?)order.AverageFillPrice ?? 0D;
					tradeBook.Amount = Math.Round((double)(tradeBook.Quantity * tradeBook.AvgFillPrice) * ledgerSign, 4);
					if (order.FilledAtUtc.HasValue) { tradeBook.FillDateTime = Util.ConvertUTCtoEST(order.FilledAtUtc.Value); }
					isTradeDone = true;
					break;
				}
				else if (OrderStatusFill.Contains(order.OrderStatus))
				{
					if (inPosition) { job.OrderID = Guid.Empty; }
					job.PositionType = (!inPosition) ? PositionType.Opened : PositionType.None;
					job.InPosition = !inPosition;
					break;
				}
			}
			job.PositionType = (inPosition) ? PositionType.Opened : PositionType.None;
			job.InPosition = inPosition;
			if (isTradeDone)
			{
				job.OrderID = Guid.Empty;
				job.PL = (inPosition) ? 0D : Math.Round(tradeBook.Amount.Value - job.PurchaseBalance, 4);
				tradeBook.ProfitLoss = job.PL;
				job.PurchaseBalance = (inPosition) ? Math.Round((tradeBook.Quantity.Value * tradeBook.AvgFillPrice), 4) : 0D;
				job.TradeBalance += tradeBook.Amount.Value;
				job.AvgFillPrice = tradeBook.AvgFillPrice;
				job.FillDate = tradeBook.FillDateTime.Value;
				job.Quantity = tradeBook.Quantity ?? 0; //Only setting this here to ensure notification includes quantity actually traded
				NotificationDTO alert = new NotificationDTO(job, Cache.NotifyFromAddress, account.NotifyRecipient);
				job.Quantity = (inPosition) ? tradeBook.Quantity ?? job.Quantity : 0;
				if (account.Notify) { await Util.RetryOnExceptionAsync(async () => await Store.PushMessageToQueue(Cache.NotificationMonitorQueueName, Store.Serialize(alert)), logger); };
			}
			else
			{
				job.OrderID = tradeBook.OrderID;
			}
			return isTradeDone;
		}

		public static async Task<List<Quote>> FetchValidateQuotes(this TradeDispatchDTO job, DateTime tradeDateStart, ILogger logger)
		{
			;
			List<Quote> niffQuotes = new List<Quote>();
			bool isValid = false;
			for (int i = 0; i < 5; i++)
			{
				try
				{
					IInclusiveTimeInterval timeInterval = TimeInterval.GetInclusiveIntervalFromThat(job.EndPreviousSessionUTC);
					IReadOnlyList<IAgg> quotes = await Util.RetryOnExceptionWithReturnAsync(async () => await Util.GetRealtimeQuoteData(job.Symbol, timeInterval), logger);
					niffQuotes = (
						from quote in quotes.OrderBy(x => x.TimeUtc)
						select new Quote()
						{
							Open = (double)quote.Open,
							High = (double)quote.High,
							Low = (double)quote.Low,
							Close = (double)quote.Close,
							Volume = (int)quote.Volume,
							Time = Util.ConvertUTCtoEST(quote.TimeUtc.Value)
						}
					).ToList();
					if (niffQuotes.Exists(x => x.Time > tradeDateStart)) { isValid = true; break; }
					await Task.Delay(TimeSpan.FromSeconds(2));
				}
				catch (Exception e)
				{
					throw e;
				}
			}
			if (!isValid)
			{
				throw new Exception($"Insufficient quote data for {job.Symbol} from {tradeDateStart}");
			}
			return niffQuotes;
		}

		public static async Task<bool> MarketBuy(this TradeDispatchDTO job, Trade trade, ILogger logger)
		{
			bool isTradeDone = false;
			IReadOnlyList<IOrder> orders; IOrder order;
			var tradingClient = Cache.TradingClient(job.AccountID);
			IAccount accountAPCA = await Util.RetryOnExceptionWithReturnAsync(async () => await tradingClient.GetAccountAsync(), logger);
			//IReadOnlyList<IPosition> positions = await Util.RetryOnExceptionWithReturnAsync(async () => await tradingClient.ListPositionsAsync(), logger);
			IPosition position = await GetPositionAsync(job, logger);
			ListOrdersRequest lor = new ListOrdersRequest() { OrderStatusFilter = OrderStatusFilter.Open, OrderListSorting = SortDirection.Ascending };
			lor.SetExclusiveTimeInterval(job.StartCurrentSessionUTC, job.EndCurrentSessionUTC);
			orders = await Util.RetryOnExceptionWithReturnAsync(async () => await tradingClient.ListOrdersAsync(lor), logger);
			if (orders.Where(x => x.Symbol == trade.Symbol).Count() > 0) { return isTradeDone; }
			if (position != null)
			{
				lor.OrderStatusFilter = OrderStatusFilter.Closed; lor.OrderListSorting = SortDirection.Descending;
				orders = await Util.RetryOnExceptionWithReturnAsync(async () => await tradingClient.ListOrdersAsync(lor), logger);
				order = orders.Where(x => x.Symbol == trade.Symbol && x.OrderSide == OrderSide.Buy).First();
				trade.LastBuyMinute = order.FilledAtUtc.Value.TimeOfDay;
				trade.LastSellMinute = TimeSpan.Zero;
				isTradeDone = true;
			}
			else
			{
				NewOrderRequest nor = new NewOrderRequest(trade.Symbol, (long)trade.Quantity, OrderSide.Buy, OrderType.Market, TimeInForce.Day);
				logger.LogInformation("Attempting Market order to Buy {qty} shares of {symbol} stock.", (long)trade.Quantity, trade.Symbol);
				order = await Util.RetryOnExceptionWithReturnAsync(async () => await tradingClient.PostOrderAsync(nor), logger);
			}
			TradeBook tradeBook = new TradeBook(Util.ConformedKey(job.WorkflowID.ToString(), job.TradeDate.Date))
			{
				Quantity = (int)order.Quantity,
				LastQuotePrice = trade.LastQuotePrice,
				LastQuoteVolume = trade.LastQuoteVolume,
				CurrentVolume = trade.CurrentVolume,
				CurrentOBV = trade.CurrentOBV,
				ServiceQueueID = job.ServiceQueueID,
				Symbol = job.Symbol,
				ePositionType = PositionType.Opened,
				WorkflowID = job.WorkflowID,
				OrderID = order.OrderId
			};
			tradeBook.SetRowKey();
			setTradeDispatchInfo(job, trade);
			if (!isTradeDone) { isTradeDone = await GetOrderStatus(job, tradeBook, (-1), true, logger); }
			await SyncTradebook(tradeBook, logger);
			return isTradeDone;
		}

		public static async Task<bool> MarketSell(this TradeDispatchDTO job, Trade trade, ILogger logger)
		{
			bool isTradeDone = false;
			IReadOnlyList<IOrder> orders; IOrder order;
			var tradingClient = Cache.TradingClient(job.AccountID);
			ListOrdersRequest lor = new ListOrdersRequest() { OrderStatusFilter = OrderStatusFilter.Open, OrderListSorting = SortDirection.Ascending };
			orders = await Util.RetryOnExceptionWithReturnAsync(async () => await tradingClient.ListOrdersAsync(lor), logger);
			lor.SetExclusiveTimeInterval(job.StartCurrentSessionUTC, job.EndCurrentSessionUTC);
			IPosition position = await GetPositionAsync(job, logger);
			if (position == null) { return false; } //Will force recheck of position manual close on next cycle
			NewOrderRequest nor = new NewOrderRequest(position.Symbol, position.Quantity, OrderSide.Sell, OrderType.Market, TimeInForce.Day);
			order = await Util.RetryOnExceptionWithReturnAsync(async () => await tradingClient.PostOrderAsync(nor), logger);
			TradeBook tradeBook = new TradeBook(Util.ConformedKey(job.WorkflowID.ToString(), job.TradeDate.Date))
			{
				Quantity = position.Quantity,
				ServiceQueueID = job.ServiceQueueID,
				Symbol = position.Symbol,
				LastQuotePrice = (double)position.AssetCurrentPrice,
				LastQuoteVolume = trade.LastQuoteVolume,
				CurrentVolume = trade.CurrentVolume,
				CurrentOBV = trade.CurrentOBV,
				ePositionType = PositionType.Closed,
				WorkflowID = job.WorkflowID,
				OrderID = order.OrderId
			};
			tradeBook.SetRowKey();
			setTradeDispatchInfo(job, trade);
			isTradeDone = await GetOrderStatus(job, tradeBook, 1, false, logger);
			await SyncTradebook(tradeBook, logger);
			return isTradeDone;
		}

		private static void setTradeDispatchInfo(TradeDispatchDTO job, Trade trade)
		{
			job.LastBuyMinute = trade.LastBuyMinute;
			job.LastSellMinute = trade.LastSellMinute;
			job.RuleGroup = trade.TradeRuleGroup;
		}

		#region RegisterPosition

		public static Trade RegisterBuyPosition(this EventData quantData, TradeRuleGroup tradeRule, BuySellCase buyCase, int ticks)
		{
			long maxQuantity = 0;
			int i;
			for (i = 0; i < 5 && ticks - i > 0; i++) { maxQuantity += quantData.IntraDayQuotes[ticks - i].Volume; }
			maxQuantity = Convert.ToInt32((maxQuantity / i) * .50D); //Take just 50% of the Last Quote Volume 5min average
			long quantity = (long)Math.Floor(quantData.TradePrincipal / quantData.LastQuotePrice);
			quantity = (maxQuantity > 0 && quantity > maxQuantity) ? maxQuantity : quantity;
			DateTime currentDT = quantData.IntraDayQuotes[ticks].Time;
			return new Trade()
			{
				Amount = quantity * quantData.LastQuotePrice * -1,
				Quantity = (int)quantity,
				LastQuoteVolume = quantData.IntraDayQuotes[ticks].Volume,
				LastQuotePrice = quantData.IntraDayQuotes[ticks].Close,
				CurrentVolume = quantData.IntraDayQuotes.Sum(x => x.Volume),
				CurrentOBV = quantData.CurrentOBV3,
				State = TradeState.Open,
				Symbol = quantData.Symbol,
				TradeDT = currentDT,
				TradeRuleGroup = tradeRule,
				TradeRuleCase = buyCase,
				LastBuyMinute = currentDT.TimeOfDay
			};
		}

		public static Trade RegisterSellPosition(this EventData quantData, int ticks, TradeRuleGroup tradeRule = TradeRuleGroup.None, BuySellCase sellCase = BuySellCase.None)
		{
			int quantity = quantData.TradeDispatch.Quantity;
			DateTime currentDT = quantData.IntraDayQuotes[ticks].Time;
			return new Trade()
			{
				Amount = quantity * quantData.LastQuotePrice,
				Quantity = quantity,
				LastQuoteVolume = quantData.LastQuoteVolume,
				CurrentVolume = quantData.CurrentVolume,
				CurrentOBV = quantData.CurrentOBV3,
				State = TradeState.Close,
				LastQuotePrice = quantData.LastQuotePrice,
				Symbol = quantData.Symbol,
				TradeDT = currentDT,
				TradeRuleGroup = tradeRule,
				TradeRuleCase = sellCase,
				LastSellMinute = currentDT.TimeOfDay
			};
		}

		#endregion RegisterPosition
	}
}