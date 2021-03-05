using System;

namespace Niffler.Data
{
	/// <summary>
	/// Encapsulates internally shared trade datums.
	/// </summary>
	public class Trade
	{
		public Trade()
		{
		}

		public double Amount { get; set; }
		public int Quantity { get; set; }
		public long LastQuoteVolume { get; set; }
		public long CurrentVolume { get; set; }
		public long CurrentOBV { get; set; }
		public TradeState State { get; set; } = TradeState.None;
		public double LastQuotePrice { get; set; }
		public string Symbol { get; set; }
		public DateTime TradeDT { get; set; }
		public TradeRuleGroup TradeRuleGroup { get; set; }
		public BuySellCase TradeRuleCase { get; set; }
		public TimeSpan LastBuyMinute { get; set; } = TimeSpan.Zero;
		public TimeSpan LastSellMinute { get; set; } = TimeSpan.Zero;
	}
}