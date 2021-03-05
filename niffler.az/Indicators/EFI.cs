using Niffler.Data;
using Niffler.Interfaces;

namespace Niffler.Indicators
{
	/// <summary>
	/// Elders Force Index: EFI = MA( VOL * (CCL - PCP) )
	/// Then single smoothed, then run through the TRIX
	/// p=period;p default(4)
	/// TRIX(SMA(EFI,p),p),p
	/// </summary>
	public class EFI : VIndicator<Indicator>
	{
		public EFI(int period = 4)
		{
			this.Period = period;
		}

		public override Indicator Calculate()
		{
			//SMA sma = new SMA(Period, false, 1);
			int qcount = this.Tuples.Length;
			double?[] tpls = new double?[qcount];
			for (int i = 0; i < qcount; i++)
			{
				if (i > 0)
				{
					tpls[i] = Tuples[i].Volume * (Tuples[i].QDatum - Tuples[i - 1].QDatum);
				}
				else
				{
					tpls[i] = 0D;
				}
			}
			for (int i = 0; i < qcount; i++) { Tuples[i].QDatum = tpls[i]; }
			//Single Smooth
			//sma.LoadSeries(Tuples.Skip(Period - 1).ToList(), IsQuotes = false);
			//Indicator result = sma.Calculate();
			//for (int i = (Period - 1); i >= 0; i--)
			//{
			//  result.Series.Insert(0, new IndicatorTuple(Tuples[i].Time, 0D, Tuples[i].Volume));
			//}
			//for (int i = 0; i < Tuples.Count(); i++) { Tuples[i].QDatum = result.Series[i].QDatum ?? 0D; }
			////Then perform the TRIX on the EFI
			T3 indicator = new T3(Period);
			indicator.LoadSeries(Tuples, IsQuotes = false);
			Indicator iSeries = indicator.Calculate();

			//int qcount = this.Quotes.Length;
			////Single Smooth
			//SMA sma = new SMA(Period, true, 1, true);
			//sma.LoadSeries(Tuples, IsQuotes = false);
			//Indicator result = sma.Calculate();
			//////Subtract 2 to account for zero-based array
			//for (int i = Period - 1; i >= 0; i--)
			//{
			//  result.Series.Insert(0, new IndicatorTuple(Tuples[i].Time, null));
			//}
			//for (int i = 0; i < Tuples.Count(); i++) { Tuples[i].QDatum = result.Series[i].QDatum ?? 0D; }
			////Double Smooth
			//sma.LoadSeries(Tuples.Skip(Period - 1).ToArray(), IsQuotes = false);
			//result = sma.Calculate();
			//for (int i = (2 * (Period - 1) - 1); i >= 0; i--)
			//{
			//  result.Series.Insert(0, new IndicatorTuple(Tuples[i].Time, null));
			//}
			//for (int i = 0; i < Tuples.Count(); i++) { Tuples[i].QDatum = result.Series[i].QDatum ?? 0D; }
			//WMA wma = new WMA(Period);
			//wma.LoadSeries(Tuples, IsQuotes = false);
			//result = wma.Calculate();
			//for (int i = 0; i < Tuples.Count(); i++) { Tuples[i].QDatum = result.Series[i].QDatum ?? 0D; }
			//TRIX indicator = new TRIX(Period, false);
			//indicator.LoadSeries(Tuples, IsQuotes = false);
			//iSeries = indicator.Calculate();
			return iSeries;
		}
	}
}