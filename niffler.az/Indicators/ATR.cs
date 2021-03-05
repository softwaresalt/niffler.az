using Niffler.Data;
using Niffler.Interfaces;
using System.Linq;

namespace Niffler.Indicators
{
	/// <summary>
	/// True Range / Average True Range
	/// </summary>
	public class ATR : VIndicator<ATRSeries>
	{
		public ATR()
		{
		}

		public ATR(int period)
		{
			this.Period = period;
		}

		/// <summary>
		/// TrueHigh = Highest of high[0] or close[-1]
		/// TrueLow = Highest of low[0] or close[-1]
		/// TR = TrueHigh - TrueLow
		/// ATR = EMA(TR)
		/// </summary>
		/// <see cref="http://www.fmlabs.com/reference/default.htm?url=TR.htm"/>
		/// <see cref="http://www.fmlabs.com/reference/default.htm?url=ATR.htm"/>
		/// <returns></returns>
		public override ATRSeries Calculate()
		{
			ATRSeries iSeries = new ATRSeries();
			iSeries.TrueHigh.Add(new IndicatorTuple(Quotes.First().Time, 0));
			iSeries.TrueLow.Add(new IndicatorTuple(Quotes.First().Time, 0));
			iSeries.TrueRange.Add(new IndicatorTuple(Quotes.First().Time, 0));
			iSeries.ATR.Add(new IndicatorTuple(Quotes.First().Time, 0));

			for (int i = 1; i < Quotes.Count(); i++)
			{
				double trueHigh = (Quotes[i].High >= Quotes[i - 1].Close) ? Quotes[i].High : Quotes[i - 1].Close;
				iSeries.TrueHigh.Add(new IndicatorTuple(Quotes[i].Time, trueHigh));
				double trueLow = (Quotes[i].Low <= Quotes[i - 1].Close) ? Quotes[i].Low : Quotes[i - 1].Close;
				iSeries.TrueLow.Add(new IndicatorTuple(Quotes[i].Time, trueLow));
				double trueRange = (trueHigh - trueLow);
				iSeries.TrueRange.Add(new IndicatorTuple(Quotes[i].Time, trueRange));
			}

			for (int i = 1; i < iSeries.TrueRange.Count(); i++)
			{
				Tuples[i].QDatum = iSeries.TrueRange[i].QDatum.Value;
			}

			EMA ema = new EMA(Period, true);
			ema.LoadSeries(Tuples.Skip(1).ToList());
			iSeries.ATR.AddRange(ema.Calculate().Series);

			return iSeries;
		}
	}
}