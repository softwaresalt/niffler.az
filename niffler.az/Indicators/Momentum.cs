using Niffler.Data;
using Niffler.Interfaces;
using System.Linq;

namespace Niffler.Indicators
{
	public class Momentum : VIndicator<Indicator>
	{
		public Momentum()
		{
		}

		public Momentum(bool isDeltaHigh)
		{
			this.isDeltaHigh = isDeltaHigh;
		}

		protected bool isDeltaHigh = false;

		public override Indicator Calculate()
		{
			Indicator iSeries = new Indicator();
			iSeries.Add(new IndicatorTuple(Tuples[0].Time, 0D));

			for (int i = 1; i < Tuples.Count(); i++)
			{
				double? qv = (isDeltaHigh) ? (Tuples[i - 1].QDatum.Value - Tuples[i].QDatum.Value) : (Tuples[i].QDatum.Value - Tuples[i - 1].QDatum.Value);
				iSeries.Add(new IndicatorTuple(Tuples[i].Time, qv));
			}

			return iSeries;
		}
	}
}