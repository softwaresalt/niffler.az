using System.Collections.Generic;

namespace Niffler.Indicators
{
	public class AroonSeries
	{
		public AroonSeries()
		{
		}

		public List<double?> Up { get; private set; } = new List<double?>();
		public List<double?> Down { get; private set; } = new List<double?>();
	}
}