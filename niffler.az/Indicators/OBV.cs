using Niffler.Data;
using Niffler.Interfaces;
using System.Linq;

namespace Niffler.Indicators
{
	/// <summary>
	/// On Balance Volume (OBV)
	/// </summary>
	public class OBV : VIndicator<Indicator>
	{
		/// <summary>
		/// If today’s close is greater than yesterday’s close then:
		/// OBV(i) = OBV(i-1)+VOLUME(i)
		/// If today’s close is less than yesterday’s close then:
		/// OBV(i) = OBV(i-1)-VOLUME(i)
		/// If today’s close is equal to yesterday’s close then:
		/// OBV(i) = OBV(i-1)
		/// </summary>
		/// <see cref="http://ta.mql4.com/indicators/volumes/on_balance_volume"/>
		/// <returns></returns>
		public override Indicator Calculate()
		{
			Indicator iSeries = new Indicator();
			iSeries.Add(new IndicatorTuple(Quotes[0].Time, Quotes[0].Volume));

			for (int i = 1; i < Quotes.Count(); i++)
			{
				double value = 0.0;
				if (Quotes[i].Close > Quotes[i - 1].Close)
				{
					value = iSeries.Series[i - 1].QDatum.Value + Quotes[i].Volume;
				}
				else if (Quotes[i].Close < Quotes[i - 1].Close)
				{
					value = iSeries.Series[i - 1].QDatum.Value - Quotes[i].Volume;
				}
				else if (Quotes[i].Close == Quotes[i - 1].Close)
				{
					value = iSeries.Series[i - 1].QDatum.Value;
				}

				iSeries.Add(new IndicatorTuple(Quotes[i].Time, value));
			}

			return iSeries;
		}
	}
}