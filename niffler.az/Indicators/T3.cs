using Niffler.Data;
using Niffler.Interfaces;
using System.Linq;

namespace Niffler.Indicators
{
	/// <summary>
	/// T3: Formula
	/// T3(n) = GD(GD(GD(n)))
	/// GD stands for Generalized DEMA(double-smoothed exponential MA); n = Period, v = Volume Factor, d = data series
	/// GD(d) = (EMA(d,n) * (1 + v)) - (EMA(EMA(d,n)) * v)
	/// GD(n, v) = EMA(n)*(1+v) - EMA(EMA(n)) * v
	/// </summary>
	public class T3 : VIndicator<Indicator>
	{
		/// <summary>
		///
		/// </summary>
		/// <param name="period">time interval/period over which to look back</param>
		/// <param name="vfactor">Typically .7, but can be modified to tweak results</param>
		/// <param name="t">T3 is typically a triple loop of the GD function; could also be T2 or T4: t=iterations of the GD function.</param>
		public T3(int period = 2, double vfactor = .5, int t = 3)
		{
			this.Period = period;
			this.Factor = vfactor;
			this.Iterations = t;
		}

		public override Indicator Calculate()
		{
			EMA ema = new EMA(Period, false);
			DEMA dema = new DEMA(Period);
			for (int i = 1; i <= this.Iterations; i++)
			{
				ema.LoadSeries(this.Tuples);
				dema.LoadSeries(this.Tuples);
				GD(ema, dema); //T(n)
			}
			Indicator iSeries = new Indicator(this.Tuples);
			return iSeries;
		}

		private void GD(EMA ema, DEMA dema)
		{
			Indicator ema1 = ema.Calculate();
			Indicator dema1 = dema.Calculate();
			for (int i = 0; i < Tuples.Count(); i++)
			{
				Tuples[i].QDatum = (ema1.Series[i].QDatum ?? 0D) * (1 + this.Factor) - ((dema1.Series[i].QDatum ?? 0D) * this.Factor);
			}
		}
	}
}