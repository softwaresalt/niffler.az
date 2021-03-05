using Alpaca.Markets;
using Newtonsoft.Json;
using System;

namespace Niffler.Data.APCA
{
	public class Quote : IAgg
	{
		[JsonConstructor]
		public Quote() { }

		public Quote(Quote quote)
		{
			this.Time = quote.Time;
			this.TimeUtc = quote.TimeUtc;
			this.Open = quote.Open;
			this.High = quote.High;
			this.Low = quote.Low;
			this.Close = quote.Close;
			this.Volume = quote.Volume;
		}

		public DateTime? Time { get; set; }
		public DateTime? TimeUtc { get; set; }

		//{
		//  get { return TimeZoneInfo.ConvertTimeFromUtc(DateTimeOffset.FromUnixTimeSeconds(UTime).DateTime, Cache.EST); }
		//  set { this.UTime = TimeZoneInfo.ConvertTimeToUtc(value, Cache.EST).Ticks / TimeSpan.TicksPerSecond; }
		//}
		//[JsonProperty(Required = Required.Always, PropertyName = "t")]
		//public long UTime { get; set; }
		[JsonProperty(Required = Required.Always, PropertyName = "o")]
		public decimal Open { get; set; }

		[JsonProperty(Required = Required.Always, PropertyName = "h")]
		public decimal High { get; set; }

		[JsonProperty(Required = Required.Always, PropertyName = "l")]
		public decimal Low { get; set; }

		[JsonProperty(Required = Required.Always, PropertyName = "c")]
		public decimal Close { get; set; }

		[JsonProperty(Required = Required.Always, PropertyName = "v")]
		public long Volume { get; set; }

		public int ItemsInWindow => throw new NotImplementedException();
	}
}