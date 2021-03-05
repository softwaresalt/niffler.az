using System.Collections.Generic;

namespace Niffler.Indicators
{
	public class RSISeries
	{
		public List<double?> RSI { get; set; }
		public List<double?> RS { get; set; }

		public RSISeries()
		{
			RSI = new List<double?>();
			RS = new List<double?>();
		}
	}
}