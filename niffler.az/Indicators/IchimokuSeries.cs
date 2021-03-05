using System.Collections.Generic;

namespace Niffler.Indicators
{
	public class IchimokuSeries
	{
		public IchimokuSeries()
		{
		}

		public List<double?> ConversionLine { get; set; } = new List<double?>();
		public List<double?> BaseLine { get; set; } = new List<double?>();
		public List<double?> LeadingSpanA { get; set; } = new List<double?>();
		public List<double?> LeadingSpanB { get; set; } = new List<double?>();
		public List<double?> LaggingSpan { get; set; } = new List<double?>();
	}
}