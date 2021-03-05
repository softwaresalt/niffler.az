using Niffler.Data;
using Niffler.Interfaces;
using System.Linq;

namespace Niffler.Indicators
{
	/// <summary>
	/// Price Volume Trend (PVT)
	/// </summary>
	public class PVT : VIndicator<Indicator>
	{
		/// <summary>
		/// PVT = [((CurrentClose - PreviousClose) / PreviousClose) x Volume] + PreviousPVT
		/// </summary>
		/// <see cref="https://www.tradingview.com/stock-charts-support/index.php/Price_Volume_Trend_(PVT)"/>
		/// <returns></returns>
		public override Indicator Calculate()
		{
			Indicator iSeries = new Indicator();
			iSeries.Add(new IndicatorTuple(Quotes[0].Time, null));

			for (int i = 1; i < Quotes.Count(); i++)
			{
				double? qv = ((((Quotes[i].Close - Quotes[i - 1].Close) / Quotes[i - 1].Close) * Quotes[i].Volume) + iSeries.Series[i - 1].QDatum);
				iSeries.Add(new IndicatorTuple(Quotes[i].Time, qv));
			}

			return iSeries;
		}
	}
}