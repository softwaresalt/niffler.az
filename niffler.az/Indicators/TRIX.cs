using Niffler.Data;
using Niffler.Interfaces;
using System.Linq;

namespace Niffler.Indicators
{
	/// <summary>
	/// Triple Smoothed Exponential Oscillator
	/// </summary>
	public class TRIX : VIndicator<Indicator>
	{
		public TRIX()
		{
		}

		public TRIX(int period = 7, bool calculatePercentage = true)
		{
			this.Period = period;
			this.CalculatePercentage = calculatePercentage;
		}

		/// <summary>
		/// 1 - EMA of Close prices [EMA(Close)]
		/// 2 - Double smooth [EMA(EMA(Close))]
		/// 3 - Triple smooth [EMA(EMA(EMA(Close)))]
		/// 4 - a) Calculation with percentage: [ROC(EMA(EMA(EMA(Close))))]
		/// 4 - b) Calculation with percentage: [Momentum(EMA(EMA(EMA(Close))))]
		/// </summary>
		/// <see cref="http://stockcharts.com/school/doku.php?id=chart_school:technical_indicators:trix"/>
		/// <see cref="http://www.fmlabs.com/reference/default.htm?url=TRIX.htm"/>
		/// <returns></returns>
		public override Indicator Calculate()
		{
			Indicator iSeries;
			// EMA calculation
			EMA ema = new EMA(Period, false);
			ema.LoadSeries(Tuples);
			var ema1 = ema.Calculate();
			for (int i = 0; i < Tuples.Count(); i++)
			{
				Tuples[i].QDatum = ema1.Series[i].QDatum ?? 0D;
			}

			// Double smooth
			ema.LoadSeries(Tuples.Skip(Period - 1).ToList());
			var ema2 = ema.Calculate();
			for (int i = Period - 2; i >= 0; i--)
			{
				ema2.Series.Insert(0, new IndicatorTuple(Tuples[i].Time, 0D));
			}
			for (int i = 0; i < Tuples.Count(); i++)
			{
				Tuples[i].QDatum = ema2.Series[i].QDatum ?? 0D;
			}

			// Triple smooth
			ema.LoadSeries(Tuples.Skip(2 * (Period - 1)).ToList());
			var ema3 = ema.Calculate();
			for (int i = (2 * (Period - 1) - 1); i >= 0; i--)
			{
				ema3.Series.Insert(0, new IndicatorTuple(Tuples[i].Time, 0D));
			}
			for (int i = 0; i < Tuples.Count(); i++)
			{
				Tuples[i].QDatum = ema3.Series[i].QDatum ?? 0D;
			}

			// Last step
			if (CalculatePercentage)
			{
				ROC roc = new ROC(1);
				roc.LoadSeries(Tuples.Skip(3 * (Period - 1)).ToList());
				iSeries = roc.Calculate();
			}
			else
			{
				Momentum momentum = new Momentum();
				momentum.LoadSeries(Tuples.Skip(3 * (Period - 1)).ToList());
				iSeries = momentum.Calculate();
			}

			for (int i = (3 * (Period - 1) - 1); i >= 0; i--)
			{
				iSeries.Series.Insert(0, new IndicatorTuple(Tuples[i].Time, 0D));
			}

			return iSeries;
		}
	}
}