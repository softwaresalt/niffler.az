using Niffler.Data;
using Niffler.Interfaces;
using System.Linq;

namespace Niffler.Indicators
{
	/// <summary>
	/// William %R
	/// </summary>
	public class WPR : VIndicator<Indicator>
	{
		public WPR()
		{
		}

		public WPR(int period)
		{
			this.Period = period;
		}

		/// <summary>
		/// %R = (Highest High - Close)/(Highest High - Lowest Low) * 100
		/// Lowest Low = lowest low for the look-back period
		/// Highest High = highest high for the look-back period
		/// %R is multiplied by -100 correct the inversion and move the decimal.
		/// </summary>
		/// <see cref="http://www.fmlabs.com/reference/default.htm?url=WilliamsR.htm"/>
		/// <returns></returns>
		public override Indicator Calculate()
		{
			Indicator iSeries = new Indicator();

			for (int i = 0; i < Quotes.Count(); i++)
			{
				if (i >= Period - 1)
				{
					double highestHigh = HighestHigh(i);
					double lowestLow = LowestLow(i);
					double wpr = (highestHigh - Quotes[i].Close) / (highestHigh - lowestLow) * (100);
					iSeries.Add(new IndicatorTuple(Quotes[i].Time, wpr));
				}
				else
				{
					iSeries.Add(new IndicatorTuple(Quotes[i].Time, null));
				}
			}

			return iSeries;
		}

		private double HighestHigh(int index)
		{
			int startIndex = index - (Period - 1);
			int endIndex = index;

			double highestHigh = 0.0;
			for (int i = startIndex; i <= endIndex; i++)
			{
				if (Quotes[i].High > highestHigh)
				{
					highestHigh = Quotes[i].High;
				}
			}

			return highestHigh;
		}

		private double LowestLow(int index)
		{
			int startIndex = index - (Period - 1);
			int endIndex = index;

			double lowestLow = double.MaxValue;
			for (int i = startIndex; i <= endIndex; i++)
			{
				if (Quotes[i].Low < lowestLow)
				{
					lowestLow = Quotes[i].Low;
				}
			}

			return lowestLow;
		}
	}
}