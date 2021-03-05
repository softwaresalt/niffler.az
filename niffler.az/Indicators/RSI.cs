using Niffler.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Niffler.Indicators
{
	/// <summary>
	/// Relative Strength Index (RSI)
	/// </summary>
	public class RSI : VIndicator<RSISeries>
	{
		private List<double?> change = new List<double?>();

		public RSI(int period)
		{
			this.Period = period;
		}

		/// <summary>
		///    RS = Average Gain / Average Loss
		///
		///                  100
		///    RSI = 100 - --------
		///                 1 + RS
		/// </summary>
		/// <see cref="http://stockcharts.com/school/doku.php?id=chart_school:technical_indicators:relative_strength_index_rsi"/>
		/// <returns></returns>
		public override RSISeries Calculate()
		{
			RSISeries rsiSeries = new RSISeries();

			// Add null values for first item, iteration will start from second item of Quotes
			rsiSeries.RS.Add(null);
			rsiSeries.RSI.Add(null);
			change.Add(null);

			for (int i = 1; i < Quotes.Count(); i++)
			{
				if (i >= this.Period)
				{
					var averageGain = change.Where(x => x > 0).Sum() / change.Count;
					var averageLoss = change.Where(x => x < 0).Sum() * (-1) / change.Count;
					var rs = averageGain / averageLoss;
					rsiSeries.RS.Add(rs);
					var rsi = 100 - (100 / (1 + rs));
					rsiSeries.RSI.Add(rsi);
					// assign change for item
					change.Add(Quotes[i].Close - Quotes[i - 1].Close);
				}
				else
				{
					rsiSeries.RS.Add(null);
					rsiSeries.RSI.Add(null);
					// assign change for item
					change.Add(Quotes[i].Close - Quotes[i - 1].Close);
				}
			}

			return rsiSeries;
		}
	}
}