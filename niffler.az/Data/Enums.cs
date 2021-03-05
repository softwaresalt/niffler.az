using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Niffler.Data
{
	public enum ServiceLoginParameterType
	{
		LoginURL = 1,
		LoginSubmitURL = 2,
		LoginUsernameFormField = 3,
		LoginPasswordFormField = 4,
		LoginRememberThisComputerFormField = 5,
		LoginUsername = 6,
		LoginPassword = 7,
		LoginRememberThisComputer = 8,
		APIEndpointURL = 9,
		APIKeyID = 10,
		APISecretKey = 11
	};

	[JsonConverter(typeof(StringEnumConverter))]
	public enum ServiceType
	{
		StockScreener,
		StockQuote,
		MarketOrderService,
		BatchMarketOrderService
	};

	public enum SettingsType
	{
		ExchangeUTCOffset
	}

	[JsonConverter(typeof(StringEnumConverter))]
	public enum OperationType
	{
		FetchScreenData,
		FetchQuoteData,
		BracketBuySell,
		MarketBuy,
		MarketSell,
		StopLimitSell,
		StopSell
	};

	public enum Boundary
	{
		T0930,
		T0945,
		T1000,
		T1030,
		T1100,
		T1240,
		T1330,
		T1400,
		T1500,
		T1530,
		T1550,
		T1600
	};

	public enum DType //Duration Type
	{
		DIInvertedDelta7,
		DIPositiveDelta7,
		DIPositiveDelta5,
		DIPositiveDelta2,
		DIPositive,
		TradesBefore1030TimeBoundary,
		MaxTradesBefore1030TimeBoundary,
		TicksAfterTrix10Low,
		PDI20DurationLimit,
		GR5Duration,
		GR6Duration,
		GR7Duration,
		GR11Duration,
		Trix25Downtrend
	};

	public enum VType //Value Type
	{
		MDIMeanDelta,
		PDIMeanDelta,
		PDIMDIMeanFactor,
		MDIPDIMeanFactor,
		PDIMDIMeanFactorRatio,
		MDIPDIMeanFactorRatio,
		PDIMDIDelta,
		MDIPDIDelta,
		Trix7Delta,
		Trix7UpSlope,
		PriceAtTrix7Low,
		VolumeAtTrix7Low
	};

	public enum Operator
	{
		LE, //Less Than Or Equal To
		GE, //Greater Than or Equal To
		LT, //Less Than
		GT //Greater Than
	};

	public enum GetServicePropertiesField
	{
		ServiceID,
		ServiceTypeID,
		ServiceName,
		ServiceURL,
		ParameterName,
		ParameterVersion,
		ParameterValue,
		ParameterLimit
	};

	public enum GetServicePropertiesParameterField
	{
		ParameterType,
		ParameterValue
	};

	public enum ServiceQueueField
	{
		ServiceQueueID,
		QueueDate,
		WorkflowID,
		StartTimeEST,
		RunByTimeEST,
		QueueItem,
		QueueTime,
		CompletionDate,
		IsCancelled,
		AccountBalance
	}

	[JsonConverter(typeof(StringEnumConverter))]
	public enum ModuleType
	{
		NifflerDispatchRoundup,
		Niffler,
		EasyInOut
	};

	public enum TradingEnvironment
	{
		Live,
		Paper
	};

	public enum PositionType
	{
		None = 0,
		Opened = 1,
		Closed = 2,
		ManualClose = 3
	};

	public enum TradeState
	{
		None,
		Open,
		Close,
		MarketEoD
	};

	public enum SecretType
	{
		AlpacaAPIKeyID,
		AlpacaAPISecretKey
	}

	public enum AlertType
	{
		Position,
		DailyPerformance
	};

	public enum FieldMapping
	{
		Workflow,
		ServiceQueue,
		PositionType,
		Symbol,
		Quantity,
		Amount
	};

	public enum ScreenDataField
	{
		WorkflowID = 0,
		ServiceQueueID = 1,
		Ordinal = 2,
		Symbol = 3
	};

	public enum QuoteDataField
	{
		WorkflowID = 0,
		ServiceQueueID = 1,
		Symbol = 2
	};

	public enum TradeDataField
	{
		WorkflowID = 0,
		ServiceQueueID = 1,
		PositionType = 2,
		Symbol = 3,
		Price = 4,
		Quantity = 5,
		Amount = 6,
		TimestampEST = 7,
		IsPaperTrade = 8
	};

	public enum TableEntityField
	{
		PartitionKey,
		RowKey,
		ServiceQueueID,
		IsEnabled,
		IsCancelled
	};

	public enum TradeRuleGroup
	{
		None,
		Group1,
		Group2,
		Group3
	};

	public enum BuySellCase
	{
		None,
		RGBuyCase1,
		RGBuyCase2,
		RGBuyCase3,
		RGSellCase1,
		RGSellCase2,
		RGSellCase3,
		RGSellCase4,
		RGSellCase5,
		RGSellCase6,
		RGSellCase7,
		RGSellCase8,
		RGSellCase9,
		CGSellCase1,
		CGSellCase2,
		CGSellCase3,
		CGSellCase4,
		CGSellCaseStopLoss,
		CGSellCaseEOD,
		CGSellCaseBackstop
	};

	public enum SignalType //SignalType
	{
		Trix25W0005,
		Trix25W0008,
		PDI20, //PlusDI20
		MDI20, //MinusDI20
		Trix7Low,
		Trix10Low,
		Trix10LowGR4,
		Trix15Low,
		Trix25Low,
		Trix15W0005Low,
		Trix15W0005High,
		Trix15W0005LowSignal,
		Trix15W0005HighSignal,
		Trix25W0005Low,
		Trix25W0008Low,
		Trix25W0005High,
		Trix25W0008High,
		Trix25W0005HighSignal,
		Trix25W0008HighSignal,
		Trix25W0005LowSignal,
		Trix25W0008LowSignal,
		PDI20Low,
		MDI20Low,
		Trix7High,
		Trix10High,
		Trix15High,
		Trix25High,
		PDI20High,
		MDI20High,
		Trix7HighSignal,
		Trix10HighSignal,
		Trix15HighSignal,
		Trix25HighSignal,
		PDI20HighSignal,
		MDI20HighSignal,
		Trix7LowSignal,
		Trix10LowSignal,
		Trix15LowSignal,
		Trix25LowSignal,
		PDI20LowSignal,
		MDI20LowSignal,
		Trix10XUnder25,
		Trix10XOver25,
		Trix10XUnder15,
		Trix10XOver15,
		Trix7XUnder15,
		Trix7XOver15,
		Trix7xDTrix15,
		Trix7x2DTrix15,
		EFI4Low,
		EFI4High,
		EFI4HighSignal,
		EFI4LowSignal,
		SVI4Low,
		SVI4High,
		SVI4HighSignal,
		SVI4LowSignal,
		T3Low,
		T3High,
		T3HighSignal,
		T3LowSignal,
		MDI20OverPDI20,
		PI20OverMDI20,
		MDI20T2OverPDI20T2,
		PI20T2OverMDI20T2,
		MDI20T3OverPDI20T3,
		PI20T3OverMDI20T3,
		MDI20TEMASlopeLow,
		MDI20TEMASlopeHigh,
		MDI20TEMASlopeHighSignal,
		MDI20TEMASlopeLowSignal,
		PDI20TEMASlopeLow,
		PDI20TEMASlopeHigh,
		PDI20TEMASlopeHighSignal,
		PDI20TEMASlopeLowSignal,
		MDI20T2Low,
		MDI20T2High,
		MDI20T2HighSignal,
		MDI20T2LowSignal,
		PDI20T2Low,
		PDI20T2High,
		PDI20T2HighSignal,
		PDI20T2LowSignal,
		MDI20T3Low,
		MDI20T3High,
		MDI20T3HighSignal,
		MDI20T3LowSignal,
		PDI20T3Low,
		PDI20T3High,
		PDI20T3HighSignal,
		PDI20T3LowSignal,
		LastBuyTick,
		LastSellTick,
		TrailingStopActivated1Pct,
		TrailingStopActivated8Pct,
		Trix7RiseAboveThreshold
	};
}