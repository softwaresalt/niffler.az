using System.Collections.Generic;

namespace Niffler.Data
{
	/// <summary>
	/// The FieldMap is used define a mapping between source field names and target field names
	/// to be used in TableStorage; e.g. "Market Cap" maps to "MarketCap", which is the literal field name: ScreenLog.MarketCap.
	/// The source field name could change depending on the provider.
	/// </summary>
	public class FieldMap
	{
		public KeyValuePair<string, string>[] ColumnMap;
		public List<KeyValuePair<string, string>> FieldValues = new List<KeyValuePair<string, string>>();

		public FieldMap(ServiceType serviceType)
		{
			switch (serviceType)
			{
				case ServiceType.StockScreener:
					ScreenLogFieldMap();
					break;

				case ServiceType.StockQuote:
					QuoteLogFieldMap();
					break;

				case ServiceType.MarketOrderService:
					SimulatedMarketOrderFieldMap();
					break;
			}
		}

		public void SetFieldValues(FieldMapping mapping, string value)
		{
			switch (mapping)
			{
				case FieldMapping.Workflow:
					FieldValues.Add(new KeyValuePair<string, string>(nameof(QuoteDataField.WorkflowID).ToLower(), value));
					break;

				case FieldMapping.ServiceQueue:
					FieldValues.Add(new KeyValuePair<string, string>(nameof(QuoteDataField.ServiceQueueID).ToLower(), value));
					break;
			}
		}

		public void SimulatedMarketOrderFieldMap()
		{
			ColumnMap = new[]
			{ //Key=Quote source field name; Value=QuoteLog table field name
				new KeyValuePair<string, string>(nameof(TradeDataField.WorkflowID).ToLower(), nameof(TradeDataField.WorkflowID)),
				new KeyValuePair<string, string>(nameof(TradeDataField.ServiceQueueID).ToLower(), nameof(TradeDataField.ServiceQueueID)),
				new KeyValuePair<string, string>(nameof(TradeDataField.PositionType).ToLower(), nameof(TradeDataField.PositionType)),
				new KeyValuePair<string, string>(nameof(TradeDataField.Symbol).ToLower(), nameof(QuoteDataField.Symbol)),
				new KeyValuePair<string, string>("close", nameof(TradeDataField.Price)),
				new KeyValuePair<string, string>(nameof(TradeDataField.Quantity).ToLower(), nameof(TradeDataField.Quantity)),
				new KeyValuePair<string, string>(nameof(TradeDataField.Amount).ToLower(), nameof(TradeDataField.Amount)),
				new KeyValuePair<string, string>("timestamp", nameof(TradeDataField.TimestampEST))
			};
		}

		public void QuoteLogFieldMap()
		{
			ColumnMap = new[]
			{ //Key=Quote source field name; Value=QuoteLog table field name
				new KeyValuePair<string, string>(nameof(QuoteDataField.WorkflowID).ToLower(), nameof(QuoteDataField.WorkflowID)),
				new KeyValuePair<string, string>(nameof(QuoteDataField.ServiceQueueID).ToLower(), nameof(QuoteDataField.ServiceQueueID)),
				new KeyValuePair<string, string>(nameof(QuoteDataField.Symbol).ToLower(), nameof(QuoteDataField.Symbol)),
				new KeyValuePair<string, string>("timestamp", "TimeStampRecorded"),
				new KeyValuePair<string, string>("close", "PriceClose"),
				new KeyValuePair<string, string>("open", "PriceOpen"),
				new KeyValuePair<string, string>("high", "PriceHigh"),
				new KeyValuePair<string, string>("low", "PriceLow"),
				new KeyValuePair<string, string>("volume", "Volume")
			};
		}

		/// <summary>
		/// Currently only valid for FinViz screen results.
		/// ToDo: Create Table to store key/value pairs for screens partitioned by screen provider.
		/// Generate fieldmap based on keys/values returned by provider.
		/// </summary>
		public void ScreenLogFieldMap()
		{
			ColumnMap = new[]
			{ //Key=Screen source field name; Value=ScreenLog table field name
				new KeyValuePair<string, string>(nameof(QuoteDataField.WorkflowID).ToLower(), nameof(QuoteDataField.WorkflowID)),
				new KeyValuePair<string, string>(nameof(QuoteDataField.ServiceQueueID).ToLower(), nameof(QuoteDataField.ServiceQueueID)),
				new KeyValuePair<string, string>("No.", "Ordinal"),
				new KeyValuePair<string, string>("Ticker", nameof(QuoteDataField.Symbol)),
				new KeyValuePair<string, string>("Company", "Company"),
				new KeyValuePair<string, string>("Sector", "Sector"),
				new KeyValuePair<string, string>("Industry", "Industry"),
				new KeyValuePair<string, string>("Country", "Country"),
				new KeyValuePair<string, string>("Market Cap", "MarketCap"),
				new KeyValuePair<string, string>("P/E", "PE"),
				new KeyValuePair<string, string>("Forward P/E", "FPE"),
				new KeyValuePair<string, string>("PEG", "PEG"),
				new KeyValuePair<string, string>("P/S", "PS"),
				new KeyValuePair<string, string>("P/B", "PB"),
				new KeyValuePair<string, string>("P/Cash", "PCash"),
				new KeyValuePair<string, string>("P/Free Cash Flow", "PFreeCashFlow"),
				new KeyValuePair<string, string>("Dividend Yield", "DividendYield"),
				new KeyValuePair<string, string>("Payout Ratio", "Payout"),
				new KeyValuePair<string, string>("EPS (ttm)", "EPS_ttm"),
				new KeyValuePair<string, string>("EPS growth this year", "EPS_GTY"),
				new KeyValuePair<string, string>("EPS growth next year", "EPS_GNY"),
				new KeyValuePair<string, string>("EPS growth past 5 years", "EPS_GP5Y"),
				new KeyValuePair<string, string>("EPS growth next 5 years", "EPS_GN5Y"),
				new KeyValuePair<string, string>("Sales growth past 5 years", "Sales_GP5Y"),
				new KeyValuePair<string, string>("EPS growth quarter over quarter", "EPS_GQoQ"),
				new KeyValuePair<string, string>("Sales growth quarter over quarter", "Sales_GQoQ"),
				new KeyValuePair<string, string>("Shares Outstanding", "SharesOut"),
				new KeyValuePair<string, string>("Shares Float", "SharesFloat"),
				new KeyValuePair<string, string>("Insider Ownership", "InsiderOwned"),
				new KeyValuePair<string, string>("Insider Transactions", "InsiderTx"),
				new KeyValuePair<string, string>("Institutional Ownership", "InstOwn"),
				new KeyValuePair<string, string>("Institutional Transactions", "InstTx"),
				new KeyValuePair<string, string>("Float Short", "FloatShort"),
				new KeyValuePair<string, string>("Short Ratio", "ShortRatio"),
				new KeyValuePair<string, string>("Return on Assets", "ROA"),
				new KeyValuePair<string, string>("Return on Equity", "ROE"),
				new KeyValuePair<string, string>("Return on Investment", "ROI"),
				new KeyValuePair<string, string>("Current Ratio", "CurrentRatio"),
				new KeyValuePair<string, string>("Quick Ratio", "QuickRatio"),
				new KeyValuePair<string, string>("LT Debt/Equity", "LT_DE"),
				new KeyValuePair<string, string>("Total Debt/Equity", "Total_DE"),
				new KeyValuePair<string, string>("Gross Margin", "GrossMargin"),
				new KeyValuePair<string, string>("Operating Margin", "OperationMargin"),
				new KeyValuePair<string, string>("Profit Margin", "ProfitMargin"),
				new KeyValuePair<string, string>("Performance (Week)", "PerfByWeek"),
				new KeyValuePair<string, string>("Performance (Month)", "PerfByMonth"),
				new KeyValuePair<string, string>("Performance (Quarter)", "PerfByQuarter"),
				new KeyValuePair<string, string>("Performance (Half Year)", "PerfByHalfYear"),
				new KeyValuePair<string, string>("Performance (Year)", "PerfByYear"),
				new KeyValuePair<string, string>("Performance (YTD)", "PerfYTD"),
				new KeyValuePair<string, string>("Beta", "Beta"),
				new KeyValuePair<string, string>("Average True Range", "AvgTrueRange"),
				new KeyValuePair<string, string>("Volatility (Week)", "VolatilityByWeek"),
				new KeyValuePair<string, string>("Volatility (Month)", "VolatilityByMonth"),
				new KeyValuePair<string, string>("20-Day Simple Moving Average", "SMA20Day"),
				new KeyValuePair<string, string>("50-Day Simple Moving Average", "SMA50Day"),
				new KeyValuePair<string, string>("200-Day Simple Moving Average", "SMA200Day"),
				new KeyValuePair<string, string>("50-Day High", "High50Day"),
				new KeyValuePair<string, string>("50-Day Low", "Low50Day"),
				new KeyValuePair<string, string>("52-Week High", "High52Week"),
				new KeyValuePair<string, string>("52-Week Low", "Low52Week"),
				new KeyValuePair<string, string>("Relative Strength Index (14)", "RSI14"),
				new KeyValuePair<string, string>("Change from Open", "CfO"),
				new KeyValuePair<string, string>("Gap", "Gap"),
				new KeyValuePair<string, string>("Analyst Recom", "AnalystRecommend"),
				new KeyValuePair<string, string>("Average Volume", "AvgVol"),
				new KeyValuePair<string, string>("Relative Volume", "RelVol"),
				new KeyValuePair<string, string>("Price", "Price"),
				new KeyValuePair<string, string>("Change", "Change"),
				new KeyValuePair<string, string>("Volume", "Volume"),
				new KeyValuePair<string, string>("Earnings Date", "EarningsDate"),
				new KeyValuePair<string, string>("Target Price", "TargetPrice"),
				new KeyValuePair<string, string>("IPO Date", "IPODate")
			};
		}
	}
}