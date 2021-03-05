using System.Collections.Generic;

namespace Niffler.Indicators
{
	public class BollingerBandSeries
	{
		public BollingerBandSeries()
		{
		}

		public List<double?> LowerBand { get; set; } = new List<double?>();
		public List<double?> MidBand { get; set; } = new List<double?>();
		public List<double?> UpperBand { get; set; } = new List<double?>();
		public List<double?> BandWidth { get; set; } = new List<double?>();
		public List<double?> BPercent { get; set; } = new List<double?>();
	}
}