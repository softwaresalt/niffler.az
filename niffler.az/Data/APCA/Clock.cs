using Alpaca.Markets;
using Newtonsoft.Json;
using System;

namespace Niffler.Data.APCA
{
	public class Clock : IClock
	{
		[JsonConstructor]
		public Clock() { }

		public Clock(IClock clock, TimeZoneInfo est)
		{
			this.IsOpen = clock.IsOpen;
			this.Timestamp = TimeZoneInfo.ConvertTime(clock.TimestampUtc, est);
			this.NextOpen = TimeZoneInfo.ConvertTime(clock.NextOpenUtc, est);
			this.NextClose = TimeZoneInfo.ConvertTime(clock.NextCloseUtc, est);
		}

		[JsonProperty(Required = Required.Always, PropertyName = "timestamp")]
		public DateTime Timestamp { get; set; }

		[JsonProperty(Required = Required.Always, PropertyName = "is_open")]
		public bool IsOpen { get; set; }

		[JsonProperty(Required = Required.Always, PropertyName = "next_open")]
		public DateTime NextOpen { get; set; }

		[JsonProperty(Required = Required.Always, PropertyName = "next_close")]
		public DateTime NextClose { get; set; }

		//[JsonProperty(Required = Required.Always, PropertyName = "timestamp")]
		public DateTime TimestampUtc { get; set; }

		//[JsonProperty(Required = Required.Always, PropertyName = "next_open")]
		public DateTime NextOpenUtc { get; set; }

		//[JsonProperty(Required = Required.Always, PropertyName = "next_close")]
		public DateTime NextCloseUtc { get; set; }
	}
}