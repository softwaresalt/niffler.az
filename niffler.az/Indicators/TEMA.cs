using Niffler.Data;
using Niffler.Interfaces;
using System.Linq;

namespace Niffler.Indicators
{
	/// <summary>
	/// Triple Exponential Moving Average (TEMA)
	/// </summary>
	public class TEMA : VIndicator<Indicator>
	{
		public TEMA(int period)
		{
			this.Period = period;
		}

		/// <summary>
		/// DEMA = 2 * EMA - EMA of EMA
		/// </summary>
		/// <see cref="http://forex-indicators.net/trend-indicators/tema"/>
		/// <returns></returns>
		public override Indicator Calculate()
		{
			double smoothingConstant = !this.Wilder ? (2.0 / (double)(Period + 1)) : (1.0 / (double)Period);
			Indicator iSeries = new Indicator();
			EMA ema = new EMA(Period, false);
			ema.LoadSeries(Tuples);
			var ema1 = ema.Calculate();

			// EMA calculation: Assign EMA values to tuple quant value
			for (int i = 0; i < Tuples.Count(); i++) { Tuples[i].QDatum = ema1.Series[i].QDatum ?? 0D; }

			// Double smooth: EMA(EMA(value))
			ema.LoadSeries(Tuples.Skip(Period - 1).ToList(), IsQuotes = false);
			var ema2 = ema.Calculate();
			for (int i = Period - 2; i >= 0; i--)
			{
				ema2.Series.Insert(0, new IndicatorTuple(Tuples[i].Time, null));
			}
			// EMA calculation: Assign EMA values to tuple quant value
			for (int i = 0; i < Tuples.Count(); i++) { Tuples[i].QDatum = ema2.Series[i].QDatum ?? 0D; } //Console.WriteLine("Time: {0}, Value: {1}", Tuples[i].Time, Tuples[i].QuantValue);

			// Triple smooth: EMA(EMA(EMA(value)))
			ema.LoadSeries(Tuples.Skip(2 * (Period - 1)).ToList(), IsQuotes = false);
			var ema3 = ema.Calculate();
			for (int i = (2 * (Period - 1) - 1); i >= 0; i--)
			{
				ema3.Series.Insert(0, new IndicatorTuple(Tuples[i].Time, null));
			}

			// EMA calculation: Assign EMA values to tuple quant value
			for (int i = 0; i < Tuples.Count(); i++) { Tuples[i].QDatum = ema3.Series[i].QDatum ?? 0D; } //Console.WriteLine("Time: {0}, Value: {1}", Tuples[i].Time, Tuples[i].QuantValue);

			// Calculate TEMA
			for (int i = 0; i < Tuples.Count(); i++)
			{
				//Should these literals be 3 instead of 2?
				if (ema1.Series[i].QDatum.HasValue && ema2.Series[i].QDatum.HasValue && ema3.Series[i].QDatum.HasValue)
				{
					Tuples[i].QDatum += (3 * (ema1.Series[i].QDatum.Value - ema2.Series[i].QDatum.Value));
					iSeries.Add(new IndicatorTuple(Tuples[i].Time, Tuples[i].QDatum));
				}
				else
				{
					iSeries.Add(new IndicatorTuple(Tuples[i].Time, null));
				}
			}

			return iSeries;
		}

		private double generateEMA(double ema, int currentTick, int tickPeriod, IndicatorTuple[] tuples, double smoothingConstant)
		{
			for (int i = currentTick - tickPeriod; i <= currentTick; i++)
			{
				ema = ((tuples[i].QDatum.Value - ema) * smoothingConstant) + ema;
			}
			return ema;
		}
	}
}