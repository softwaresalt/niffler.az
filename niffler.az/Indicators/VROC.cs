using Niffler.Data;
using Niffler.Interfaces;
using System.Linq;

namespace Niffler.Indicators
{
	/// <summary>
	/// Volume Rate of Change (VROC)
	/// </summary>
	public class VROC : VIndicator<Indicator>
	{
		public VROC(int period)
		{
			this.Period = period;
		}

		/// <summary>
		/// VROC = ((VOLUME (i) - VOLUME (i - n)) / VOLUME (i - n)) * 100
		/// </summary>
		/// <see cref="http://ta.mql4.com/indicators/volumes/rate_of_change"/>
		/// <returns></returns>
		public override Indicator Calculate()
		{
			Indicator iSeries = new Indicator();

			for (int i = 0; i < Quotes.Count(); i++)
			{
				if (i >= this.Period)
				{
					double? qv = ((Quotes[i].Volume - Quotes[i - this.Period].Volume) / Quotes[i - this.Period].Volume) * 100;
					iSeries.Add(new IndicatorTuple(Quotes[i].Time, qv));
				}
				else
				{
					iSeries.Add(new IndicatorTuple(Quotes[i].Time, null));
				}
			}

			return iSeries;
		}
	}
}