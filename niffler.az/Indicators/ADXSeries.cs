using Niffler.Data;
using System;
using System.Collections.Generic;

namespace Niffler.Indicators
{
	public class ADXSeries
	{
		public ADXSeries()
		{
		}

		public List<IndicatorTuple> TrueRange { get; set; } = new List<IndicatorTuple>();
		public List<IndicatorTuple> DINegative { get; set; } = new List<IndicatorTuple>();
		public List<IndicatorTuple> DIPositive { get; set; } = new List<IndicatorTuple>();
		public List<IndicatorTuple> DX { get; set; } = new List<IndicatorTuple>();
		public List<IndicatorTuple> ADX { get; set; } = new List<IndicatorTuple>();
		public List<DateTime> Time { get; set; } = new List<DateTime>();
	}
}