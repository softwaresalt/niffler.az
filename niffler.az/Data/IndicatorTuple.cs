using Niffler.Interfaces;
using System;

namespace Niffler.Data
{
	public class IndicatorTuple : IIndicatorTuple
	{
		public DateTime Time { get; set; }
		public double? QDatum { get; set; }
		public int Volume { get; set; } = 0;

		public IndicatorTuple()
		{
		}

		public IndicatorTuple(DateTime dateTime)
		{
			this.Time = dateTime;
		}

		public IndicatorTuple(DateTime dateTime, double? value, int volume = 0)
		{
			this.Time = dateTime;
			this.QDatum = value;
			this.Volume = volume;
		}
	}
}