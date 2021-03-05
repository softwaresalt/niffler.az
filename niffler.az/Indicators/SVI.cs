using Niffler.Data;
using Niffler.Interfaces;
using System.Linq;

namespace Niffler.Indicators
{
	/// <summary>
	/// Smoothed Volume Indicator (SVI)
	/// </summary>
	public class SVI : VIndicator<Indicator>
	{
		public SVI(int period = 4)
		{
			this.Period = period;
		}

		public override Indicator Calculate()
		{
			//Single Smooth
			SMA sma = new SMA(Period, true, 1, true);
			sma.LoadSeries(Tuples, IsQuotes = false);
			Indicator result = sma.Calculate();
			////Subtract 2 to account for zero-based array
			for (int i = Period - 1; i >= 0; i--)
			{
				result.Series.Insert(0, new IndicatorTuple(Tuples[i].Time, null));
			}
			for (int i = 0; i < Tuples.Count(); i++) { Tuples[i].QDatum = result.Series[i].QDatum ?? 0D; }
			//Double Smooth
			sma.LoadSeries(Tuples.Skip(Period - 1).ToArray(), IsQuotes = false);
			result = sma.Calculate();
			for (int i = (2 * (Period - 1) - 1); i >= 0; i--)
			{
				result.Series.Insert(0, new IndicatorTuple(Tuples[i].Time, null));
			}
			for (int i = 0; i < Tuples.Count(); i++) { Tuples[i].QDatum = result.Series[i].QDatum ?? 0D; }
			WMA wma = new WMA(Period);
			wma.LoadSeries(Tuples, IsQuotes = false);
			result = wma.Calculate();
			for (int i = 0; i < Tuples.Count(); i++) { Tuples[i].QDatum = result.Series[i].QDatum ?? 0D; }
			TRIX indicator = new TRIX(Period, false);
			indicator.LoadSeries(Tuples, IsQuotes = false);
			Indicator iSeries = indicator.Calculate();

			return iSeries;
		}
	}
}