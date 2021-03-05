using Niffler.Data;
using Niffler.Interfaces;
using System.Linq;

namespace Niffler.Indicators
{
	/// <summary>
	/// Double Exponential Moving Average (DEMA)
	/// </summary>
	public class DEMA : VIndicator<Indicator>
	{
		public DEMA(int period)
		{
			this.Period = period;
		}

		/// <summary>
		/// DEMA = 2 * EMA - EMA of EMA
		/// </summary>
		/// <see cref="http://forex-indicators.net/trend-indicators/dema"/>
		/// <returns></returns>
		public override Indicator Calculate()
		{
			Indicator iSeries = new Indicator();
			EMA ema = new EMA(Period, false);
			ema.LoadSeries(Tuples);
			var ema1 = ema.Calculate();

			// assign EMA values to Close price
			for (int i = 0; i < Tuples.Count(); i++)
			{
				Tuples[i].QDatum = ema1.Series[i].QDatum ?? 0.0;
				//Tuples[i].Volume = ema1.Series[i].Volume;
			}

			ema.LoadSeries(Tuples.Skip(Period - 1).ToList());
			// EMA(EMA(value))
			var ema2 = ema.Calculate();
			for (int i = Period - 2; i >= 0; i--)
			{
				ema2.Series.Insert(0, new IndicatorTuple(Tuples[i].Time, null, Tuples[i].Volume));
			}

			// Calculate DEMA: DEMA(p) = 2 * EMA(p) - EMA(EMA(p)) where p=period
			for (int i = 0; i < Tuples.Count(); i++)
			{
				if (i >= 2 * Period - 2)
				{
					double? qv = (2 * ema1.Series[i].QDatum.Value - ema2.Series[i].QDatum.Value);
					iSeries.Add(new IndicatorTuple(Tuples[i].Time, qv, Tuples[i].Volume));
				}
				else
				{
					iSeries.Add(new IndicatorTuple(Tuples[i].Time, null, Tuples[i].Volume));
				}
			}

			return iSeries;
		}
	}
}