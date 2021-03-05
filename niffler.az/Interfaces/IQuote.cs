using System;

namespace Niffler.Interfaces
{
	internal interface IQuote
	{
		DateTime Time { get; set; }
		long UTime { get; set; }
		double Open { get; set; }
		double High { get; set; }
		double Low { get; set; }
		double Close { get; set; }
		int Volume { get; set; }
	}
}