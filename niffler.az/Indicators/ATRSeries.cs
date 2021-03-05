using Niffler.Data;
using System.Collections.Generic;

namespace Niffler.Indicators
{
	public class ATRSeries
	{
		public ATRSeries()
		{
		}

		public List<IndicatorTuple> TrueHigh { get; private set; } = new List<IndicatorTuple>();
		public List<IndicatorTuple> TrueLow { get; private set; } = new List<IndicatorTuple>();
		public List<IndicatorTuple> TrueRange { get; private set; } = new List<IndicatorTuple>();
		public List<IndicatorTuple> ATR { get; private set; } = new List<IndicatorTuple>();
	}
}