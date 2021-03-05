using Niffler.Interfaces;
using System;

namespace Niffler.Data
{
	/// <summary>
	/// Encapsulates quote datums for in-app use.
	/// </summary>
	public class Quote : IQuote
	{
		public Quote()
		{
		}

		public Quote(Quote quote)
		{
			this.Time = quote.Time;
			this.Open = quote.Open;
			this.High = quote.High;
			this.Low = quote.Low;
			this.Close = quote.Close;
			this.Volume = quote.Volume;
		}

		public DateTime Time { get; set; }
		public long UTime { get; set; }
		public double Open { get; set; }
		public double High { get; set; }
		public double Low { get; set; }
		public double Close { get; set; }
		public int Volume { get; set; }
	}
}