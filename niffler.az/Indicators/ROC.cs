using Niffler.Data;
using Niffler.Interfaces;
using System.Linq;

namespace Niffler.Indicators
{
	/// <summary>
	/// Rate of Change (ROC)
	/// </summary>
	public class ROC : VIndicator<Indicator>
	{
		public ROC(int period)
		{
			this.Period = period;
		}

		/// <summary>
		/// ROC = [(Close - Close n periods ago) / (Close n periods ago)] * 100
		/// </summary>
		/// <see cref="http://stockcharts.com/school/doku.php?id=chart_school:technical_indicators:rate_of_change_roc_and_momentum"/>
		/// <returns></returns>
		public override Indicator Calculate()
		{
			Indicator iSeries = new Indicator();

			for (int i = 0; i < Tuples.Count(); i++)
			{
				if (i >= this.Period)
				{
					double? qv = (((Tuples[i].QDatum - Tuples[i - this.Period].QDatum) / Tuples[i - this.Period].QDatum) * 100);
					iSeries.Add(new IndicatorTuple(Tuples[i].Time, qv));
				}
				else
				{
					iSeries.Add(new IndicatorTuple(Tuples[i].Time, 0D));
				}
			}
			return iSeries;
		}
	}
}