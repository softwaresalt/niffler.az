using Niffler.Data;
using Niffler.Interfaces;
using System.Linq;

namespace Niffler.Indicators
{
	public class ZLEMA : VIndicator<Indicator>
	{
		public ZLEMA()
		{
		}

		public ZLEMA(int period)
		{
			this.Period = period;
		}

		public override Indicator Calculate()
		{
			Indicator iSeries = new Indicator();

			double ratio = 2.0 / (double)(Period + 1);
			double lag = 1 / ratio;
			double wt = lag - ((int)lag / 1.0) * 1.0; //DMOD( lag, 1.0D0 )
			double meanOfFirstPeriod = Tuples.Take(Period).Select(x => x.QDatum.Value).Sum() / Period;

			for (int i = 0; i < Tuples.Count(); i++)
			{
				if (i > Period - 1)
				{
					int loc = (int)(i - lag);
					double? qv = ratio * (2 * Tuples[i].QDatum - (Tuples[loc].QDatum * (1 - wt) + Tuples[loc + 1].QDatum * wt)) + (1 - ratio) * iSeries.Series[i - 1].QDatum.Value;
					iSeries.Add(new IndicatorTuple(Tuples[i].Time, qv));
				}
				else if (i == Period - 1)
				{
					double? qv = meanOfFirstPeriod;
					iSeries.Add(new IndicatorTuple(Tuples[i].Time, qv));
				}
				else
				{
					iSeries.Add(new IndicatorTuple(Tuples[i].Time, null));
				}
			}

			return iSeries;
		}
	}
}