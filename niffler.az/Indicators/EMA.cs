using Niffler.Data;
using Niffler.Interfaces;
using System.Linq;

namespace Niffler.Indicators
{
	/// <summary>
	/// Exponential Moving Average
	/// </summary>
	public class EMA : VIndicator<Indicator>
	{
		public EMA()
		{
		}

		public EMA(int period, bool wilder, bool usePeriodChangeTime = false, int periodChangeAmount = 0)
		{
			this.Period = period;
			this.Wilder = wilder;
			this.UsePeriodChangeTime = usePeriodChangeTime;
			this.PeriodChangeAmount = periodChangeAmount;
		}

		/// <summary>
		/// SMA: 10 period sum / 10
		/// Multiplier: (2 / (Time periods + 1) ) = (2 / (10 + 1) ) = 0.1818 (18.18%)
		/// EMA: {Close - EMA(previous day)} x multiplier + EMA(previous day).
		/// for Wilder parameter details: http://www.inside-r.org/packages/cran/TTR/docs/GD
		/// </summary>
		/// <see cref="http://stockcharts.com/school/doku.php?id=chart_school:technical_indicators:moving_averages"/>
		/// <returns></returns>
		public override Indicator Calculate()
		{
			Indicator iSeries = new Indicator();
			double smoothingConstant = !this.Wilder ? (2.0 / (double)(Period + 1)) : (1.0 / (double)Period);
			int periodPlus = Period + PeriodChangeAmount;

			for (int i = 0; i < Tuples.Count(); i++)
			{
				if (i >= Period - 1)
				{
					if (UsePeriodChangeTime && Period != periodPlus && Tuples[i].Time.TimeOfDay > this.PeriodChangeTime) { Period = periodPlus; }
					if (iSeries.Series[i - 1].QDatum.HasValue)
					{
						double emaPrev = iSeries.Series[i - 1].QDatum.Value;
						//double qv = (Quotes[i].Close * smoothingConstant) + (emaPrev * (1 - smoothingConstant));
						double? qv = (Tuples[i].QDatum.Value - emaPrev) * smoothingConstant + emaPrev; //This formula's results match AlphaVantage results
						iSeries.Add(new IndicatorTuple(Tuples[i].Time, qv, Tuples[i].Volume));
					}
					else
					{
						if (this.IsQuotes)
						{
							//First value is simple SMA to seed the EMA
							double sum = 0;
							for (int j = i; j >= i - (Period - 1); j--)
							{
								sum += Tuples[j].QDatum.Value;
							}
							double qv = (sum / Period);
							iSeries.Add(new IndicatorTuple(Tuples[i].Time, qv, Tuples[i].Volume));
						}
						else
						{
							iSeries.Add(new IndicatorTuple(Tuples[i].Time, Tuples[i].QDatum ?? 0D, Tuples[i].Volume));
						}
					}
				}
				else
				{
					iSeries.Add(new IndicatorTuple(Tuples[i].Time, 0D, Tuples[i].Volume));
				}
			}

			return iSeries;
		}
	}
}