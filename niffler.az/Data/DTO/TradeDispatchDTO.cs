using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace Niffler.Data
{
	/// <summary>
	/// Encapsulates trade dispatch attributes for in-app use.
	/// Designed to be serializable to JSON for storage queue.
	/// </summary>
	public class TradeDispatchDTO
	{
		[JsonConstructor]
		public TradeDispatchDTO() { }

		public TradeDispatchDTO(ServiceQueueDTO.TaskDTO task, ModuleType moduleType)
		{
			this.AccountID = task.AccountID;
			this.ModuleType = moduleType;
			this.PositionType = PositionType.None;
			this.ServiceQueueID = task.ServiceQueueID;
			this.WorkflowID = task.WorkflowID;
			this.TradeDate = task.QueueDate;
		}

		[JsonProperty(Required = Required.Always, PropertyName = "ActID")]
		public long AccountID { get; set; }

		[JsonProperty(Required = Required.Always, PropertyName = "ActNm")]
		public string AccountName { get; set; }

		[JsonProperty(Required = Required.Always, PropertyName = "SvcQID")]
		public long ServiceQueueID { get; set; }

		[JsonProperty(Required = Required.Always, PropertyName = "WFID")]
		public long WorkflowID { get; set; }

		[JsonProperty(Required = Required.Always, PropertyName = "TrdDt")]
		public DateTime TradeDate { get; set; }

		[JsonProperty(Required = Required.Always, PropertyName = "ModT")]
		[JsonConverter(typeof(StringEnumConverter))]
		public ModuleType ModuleType { get; set; }

		[JsonProperty(Required = Required.Always, PropertyName = "Pos")]
		public bool InPosition { get; set; } = false;

		[JsonProperty(Required = Required.Always, PropertyName = "ShrtPos")]
		public bool InShortPosition { get; set; } = false;

		[JsonProperty(Required = Required.Always, PropertyName = "Shrt")]
		public bool IsShortable { get; set; } = false;

		[JsonProperty(Required = Required.Always, PropertyName = "TBal")]
		public double TradeBalance { get; set; } = 0D;

		[JsonProperty(Required = Required.Always, PropertyName = "PBal")]
		public double PurchaseBalance { get; set; } = 0D;

		[JsonProperty(Required = Required.Always, PropertyName = "PosT")]
		[JsonConverter(typeof(StringEnumConverter))]
		public PositionType PositionType { get; set; } = PositionType.None;

		[JsonProperty(Required = Required.Always, PropertyName = "Smbl")]
		public string Symbol { get; set; }

		[JsonProperty(Required = Required.Always, PropertyName = "Qty")]
		public int Quantity { get; set; } = 0;

		[JsonProperty(Required = Required.Always, PropertyName = "EPS")]
		public DateTime EndPreviousSessionUTC { get; set; }

		[JsonProperty(Required = Required.Always, PropertyName = "SCS")]
		public DateTime StartCurrentSessionUTC { get; set; }

		[JsonProperty(Required = Required.Always, PropertyName = "ECS")]
		public DateTime EndCurrentSessionUTC { get; set; }

		[JsonProperty(Required = Required.Always, PropertyName = "RG")]
		[JsonConverter(typeof(StringEnumConverter))]
		public TradeRuleGroup RuleGroup { get; set; } = TradeRuleGroup.None;

		[JsonProperty(Required = Required.Always, PropertyName = "TBTB")]
		public int TradesBeforeTimeBoundary { get; set; } = 0;

		[JsonProperty(Required = Required.Always, PropertyName = "LBM")]
		public TimeSpan LastBuyMinute { get; set; } = TimeSpan.Zero;

		[JsonProperty(Required = Required.Always, PropertyName = "LSM")]
		public TimeSpan LastSellMinute { get; set; } = TimeSpan.Zero;

		[JsonProperty(Required = Required.Always, PropertyName = "OID")]
		public Guid OrderID { get; set; } = Guid.Empty;

		[JsonIgnore]
		public double PL { get; set; } = 0; //Profit/Loss Amount; mostly for messaging purposes

		[JsonIgnore]
		public double AvgFillPrice { get; set; } = 0;

		[JsonIgnore]
		public DateTime FillDate { get; set; }
	}
}