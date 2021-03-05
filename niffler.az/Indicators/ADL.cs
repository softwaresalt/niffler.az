using Niffler.Data;
using Niffler.Interfaces;

namespace Niffler.Indicators
{
	/// <summary>
	/// Accumulation / Distribution Line
	/// </summary>
	public class ADL : VIndicator<Indicator>
	{
		/// <summary>
		/// Acc/Dist = ((Close – Low) – (High – Close)) / (High – Low) * Period's volume
		/// </summary>
		/// <see cref="http://www.investopedia.com/terms/a/accumulationdistribution.asp"/>
		/// <returns></returns>
		public override Indicator Calculate()
		{
			Indicator iSeries = new Indicator();
			foreach (var quote in Quotes)
			{
				double? qv = ((quote.Close - quote.Low) - (quote.High - quote.Close)) / (quote.High - quote.Low) * quote.Volume;
				iSeries.Add(new IndicatorTuple(quote.Time, qv));
			}

			return iSeries;
		}
	}
}