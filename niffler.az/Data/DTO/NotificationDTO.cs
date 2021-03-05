using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace Niffler.Data
{
	/// <summary>
	/// Encapsulates notification attributes for in-app use.
	/// Designed to be serializable to JSON for storage queue.
	/// </summary>
	public class NotificationDTO
	{
		[JsonConstructor]
		public NotificationDTO() { }

		public NotificationDTO(TradeDispatchDTO dto, string fromAddress, string toAddress)
		{
			this.AccountName = dto.AccountName;
			this.Symbol = dto.Symbol;
			this.PositionType = (dto.PositionType == PositionType.None) ? PositionType.Closed : dto.PositionType;
			this.Quantity = dto.Quantity;
			this.PL = dto.PL;
			this.FillDate = Util.ConformAsDateTimeString(dto.FillDate);
			this.FillPrice = dto.AvgFillPrice;
			this.FromAddress = fromAddress;
			this.ToAddress = toAddress;
		}

		[JsonProperty(Required = Required.Always)]
		[JsonConverter(typeof(StringEnumConverter))]
		public PositionType PositionType { get; set; } = PositionType.None;

		[JsonProperty(Required = Required.Always)]
		public string Symbol { get; set; } = String.Empty;

		public string AccountName { get; set; } = String.Empty;

		[JsonProperty(Required = Required.Always)]
		public int Quantity { get; set; } = 0;

		public double PL { get; set; } = 0D; //Profit/Loss Amount
		public double PLPct { get; set; } = 0D; //Profit/Loss Amount

		[JsonProperty(Required = Required.Always)]
		public double FillPrice { get; set; } = 0;

		[JsonProperty(Required = Required.Always)]
		public string FillDate { get; set; } = Util.ConformedDateTimeEST;

		[JsonProperty(Required = Required.Always)]
		public string FromAddress { get; set; }

		[JsonProperty(Required = Required.Always)]
		public string ToAddress { get; set; }

		[JsonProperty(Required = Required.Always)]
		public AlertType AlertType { get; set; } = AlertType.Position;
	}
}