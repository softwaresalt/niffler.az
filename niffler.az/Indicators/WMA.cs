using Niffler.Data;
using Niffler.Interfaces;
using System.Linq;

namespace Niffler.Indicators
{
	/// <summary>
	/// Weighted Moving Average
	/// </summary>
	public class WMA : VIndicator<Indicator>
	{
		public WMA(int period)
		{
			this.Period = period;
		}

		/// <summary>
		/// Therefore the 5 Day WMA is 83(5/15) + 81(4/15) + 79(3/15) + 79(2/15) + 77(1/15) = 80.7
		/// Day	     1	2	3	4	5 (current)
		/// Price	77	79	79	81	83
		/// WMA	 	 	 	 	    80.7
		/// </summary>
		/// <see cref="http://fxtrade.oanda.com/learn/forex-indicators/weighted-moving-average"/>
		/// <returns></returns>
		public override Indicator Calculate()
		{
			Indicator iSeries = new Indicator();

			int weightSum = 0;
			for (int i = 1; i <= Period; i++)
			{
				weightSum += i;
			}

			for (int i = 0; i < Tuples.Count(); i++)
			{
				if (i >= Period - 1)
				{
					double wma = 0.0;
					int weight = 1;
					for (int j = i - (Period - 1); j <= i; j++)
					{
						wma += ((double)weight / weightSum) * Tuples[j].QDatum.Value;
						weight++;
					}
					iSeries.Add(new IndicatorTuple(Tuples[i].Time, wma));
				}
				else
				{
					iSeries.Add(new IndicatorTuple(Tuples[i].Time, 0D));
				}
			}

			return iSeries;
		}
	}
}