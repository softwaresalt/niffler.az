using Niffler.Data;
using Niffler.Interfaces;
using System.Linq;

namespace Niffler.Indicators
{
	/// <summary>
	/// Simple Moving Average
	/// </summary>
	public class SMA : VIndicator<Indicator>
	{
		public SMA(int period, bool usePeriodChangeTime = false, int periodChangeAmount = 0, bool useVolume = false)
		{
			this.Period = period;
			this.UsePeriodChangeTime = usePeriodChangeTime;
			this.PeriodChangeAmount = periodChangeAmount;
			this.UseVolume = useVolume;
		}

		/// <summary>
		/// Daily Closing Prices: 11,12,13,14,15,16,17
		/// First day of 5-day SMA: (11 + 12 + 13 + 14 + 15) / 5 = 13
		/// Second day of 5-day SMA: (12 + 13 + 14 + 15 + 16) / 5 = 14
		/// Third day of 5-day SMA: (13 + 14 + 15 + 16 + 17) / 5 = 15
		/// </summary>
		/// <see cref="http://stockcharts.com/school/doku.php?id=chart_school:technical_indicators:moving_averages"/>
		/// <returns></returns>
		public override Indicator Calculate()
		{
			Indicator iSeries = new Indicator();
			int periodPlus = Period + PeriodChangeAmount;
			for (int i = 0; i < Tuples.Count(); i++)
			{
				if (UsePeriodChangeTime && Period != periodPlus && Tuples[i].Time.TimeOfDay > this.PeriodChangeTime) { Period = periodPlus; }
				if (i >= Period - 1 && (i - Period - 1) >= 0)
				{
					double sum = 0;
					for (int j = i; j >= (i - Period - 1); j--)
					{
						sum += (UseVolume) ? Tuples[j].Volume : Tuples[j].QDatum ?? 0D;
					}
					double qv = (sum / Period); //Average
					iSeries.Add(new IndicatorTuple(Tuples[i].Time, qv));
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