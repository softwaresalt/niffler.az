using Niffler.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using es = Niffler.Data.EventState;

namespace Niffler.Modules
{
	/// <summary>
	/// EventMonitor checks/set/evaluates all indicator signal states
	/// </summary>
	public class EventMonitor
	{
		public EventMonitor(EventData data, es state)
		{
			TickMonitor = new Dictionary<SignalType, int>(92)
			{
				{ SignalType.Trix7Low, -1 },{ SignalType.Trix7High, -1 },{ SignalType.Trix7LowSignal, -1 },{ SignalType.Trix7HighSignal, -1 },
				{ SignalType.Trix10Low, -1 },{ SignalType.Trix10High, -1 },{ SignalType.Trix10LowSignal, -1 },{ SignalType.Trix10HighSignal, -1 },
				{ SignalType.Trix15Low, -1 },{ SignalType.Trix15High, -1 },{ SignalType.Trix15LowSignal, -1 },{ SignalType.Trix15HighSignal, -1 },
				{ SignalType.Trix15W0005Low, -1 },{ SignalType.Trix15W0005High, -1 },{ SignalType.Trix15W0005LowSignal, -1 },{ SignalType.Trix15W0005HighSignal, -1 },
				{ SignalType.Trix25Low, -1 },{ SignalType.Trix25High, -1 },{ SignalType.Trix25LowSignal, -1 },{ SignalType.Trix25HighSignal, -1 },
				{ SignalType.Trix25W0005, -1 },{ SignalType.Trix25W0005Low, -1 },{ SignalType.Trix25W0005High, -1 },{ SignalType.Trix25W0005LowSignal, -1 },{ SignalType.Trix25W0005HighSignal, -1 },
				{ SignalType.Trix25W0008, -1 },{ SignalType.Trix25W0008Low, -1 },{ SignalType.Trix25W0008High, -1 },{ SignalType.Trix25W0008LowSignal, -1 },{ SignalType.Trix25W0008HighSignal, -1 },
				{ SignalType.PDI20, -1 },{ SignalType.PDI20Low, -1 },{ SignalType.PDI20High, -1 },{ SignalType.PDI20LowSignal, -1 },{ SignalType.PDI20HighSignal, -1 },
				{ SignalType.MDI20, -1 },{ SignalType.MDI20Low, -1 },{ SignalType.MDI20High, -1 },{ SignalType.MDI20LowSignal, -1 },{ SignalType.MDI20HighSignal, -1 },
				{ SignalType.MDI20OverPDI20, -1 },{ SignalType.PI20OverMDI20, -1 },{ SignalType.MDI20T2OverPDI20T2, -1 },{ SignalType.PI20T2OverMDI20T2, -1 },{ SignalType.MDI20T3OverPDI20T3, -1 },{ SignalType.PI20T3OverMDI20T3, -1 },
				{ SignalType.MDI20TEMASlopeLow, -1 },{ SignalType.MDI20TEMASlopeHigh, -1 },{ SignalType.MDI20TEMASlopeHighSignal, -1 },{ SignalType.MDI20TEMASlopeLowSignal, -1 },
				{ SignalType.PDI20TEMASlopeLow, -1 },{ SignalType.PDI20TEMASlopeHigh, -1 },{ SignalType.PDI20TEMASlopeHighSignal, -1 },{ SignalType.PDI20TEMASlopeLowSignal, -1 },
				{ SignalType.PDI20T2Low, -1 },{ SignalType.PDI20T2High, -1 },{ SignalType.PDI20T2HighSignal, -1 },{ SignalType.PDI20T2LowSignal, -1 },
				{ SignalType.MDI20T2Low, -1 },{ SignalType.MDI20T2High, -1 },{ SignalType.MDI20T2HighSignal, -1 },{ SignalType.MDI20T2LowSignal, -1 },
				{ SignalType.PDI20T3Low, -1 },{ SignalType.PDI20T3High, -1 },{ SignalType.PDI20T3HighSignal, -1 },{ SignalType.PDI20T3LowSignal, -1 },
				{ SignalType.MDI20T3Low, -1 },{ SignalType.MDI20T3High, -1 },{ SignalType.MDI20T3HighSignal, -1 },{ SignalType.MDI20T3LowSignal, -1 },
				{ SignalType.EFI4Low, -1 },{ SignalType.EFI4High, -1 },{ SignalType.EFI4HighSignal, -1 },{ SignalType.EFI4LowSignal, -1 },
				{ SignalType.SVI4Low, -1 },{ SignalType.SVI4High, -1 },{ SignalType.SVI4HighSignal, -1 },{ SignalType.SVI4LowSignal, -1 },
				{ SignalType.T3Low, -1 },{ SignalType.T3High, -1 },{ SignalType.T3HighSignal, -1 },{ SignalType.T3LowSignal, -1 },
				{ SignalType.Trix10XUnder25, -1 },{ SignalType.Trix10XOver25, -1 },
				{ SignalType.Trix10XUnder15, -1 },{ SignalType.Trix10XOver15, -1 },
				{ SignalType.Trix7XUnder15, -1 },{ SignalType.Trix7XOver15, -1 },
				{ SignalType.LastBuyTick, -1 },{ SignalType.LastSellTick, -1 },
				{ SignalType.Trix10LowGR4, -1 }
			};
			DurationMonitor = new Dictionary<DType, int>(11)
			{
				{ DType.DIPositive, 0 },
				{ DType.DIPositiveDelta2, 0 },
				{ DType.DIPositiveDelta5, 0 },
				{ DType.DIPositiveDelta7, 0 },
				{ DType.DIInvertedDelta7, 0 },
				{ DType.TradesBefore1030TimeBoundary, 0 },
				{ DType.MaxTradesBefore1030TimeBoundary, 1 },
				{ DType.TicksAfterTrix10Low, 1 },
				{ DType.PDI20DurationLimit, 20 },
				{ DType.Trix25Downtrend, 70 },
				{ DType.GR5Duration, 23 },
				{ DType.GR6Duration, 29 },
				{ DType.GR7Duration, 30 },
				{ DType.GR11Duration, 30 }
			};
			ValueMonitor = new Dictionary<VType, double>(3)
			{
				{ VType.MDIMeanDelta, 0 },
				{ VType.PDIMeanDelta, 0 },
				{ VType.MDIPDIMeanFactor, 0 },
				{ VType.PDIMDIMeanFactor, 0 },
				{ VType.PDIMDIMeanFactorRatio, 0 },
				{ VType.MDIPDIMeanFactorRatio, 0 },
				{ VType.Trix7Delta, 0 },
				{ VType.Trix7UpSlope, 0D }
			};
			this.Data = data;
			this.CurrentState = state;
		}

		public const double TrixDelta = .04D, Wobble0005 = .0005D, Wobble0008 = .0008D, TEMASlopeWobble = .1D, DIWobble = .2D, StopLossPct = -3.0D, DIMean = 25D;
		public Dictionary<SignalType, int> TickMonitor { get; set; }
		public Dictionary<DType, int> DurationMonitor { get; set; }
		public Dictionary<VType, double> ValueMonitor { get; set; }
		public EventData Data { get; set; }
		public es CurrentState { get; set; }
		public DateTime CurrentDT { get; set; }
		public TimeSpan CurrentTime { get; set; }
		public int Ticks { get; set; } = -1;

		private void checkSetCurrentContext(int ticks)
		{
			checkSetIndicatorLow(Data.MDI20TEMASlope, SignalType.MDI20TEMASlopeLowSignal, SignalType.MDI20TEMASlopeHighSignal, SignalType.MDI20TEMASlopeLow, SignalType.MDI20TEMASlopeHigh, 1, TEMASlopeWobble);
			checkSetIndicatorHigh(Data.MDI20TEMASlope, Data.MDI20SlopePeakTracker, SignalType.MDI20TEMASlopeLowSignal, SignalType.MDI20TEMASlopeHighSignal, SignalType.MDI20TEMASlopeLow, SignalType.MDI20TEMASlopeHigh, 1, TEMASlopeWobble);
			checkSetIndicatorLow(Data.PDI20TEMASlope, SignalType.PDI20TEMASlopeLowSignal, SignalType.PDI20TEMASlopeHighSignal, SignalType.PDI20TEMASlopeLow, SignalType.PDI20TEMASlopeHigh, 1, TEMASlopeWobble);
			checkSetIndicatorHigh(Data.PDI20TEMASlope, SignalType.PDI20TEMASlopeLowSignal, SignalType.PDI20TEMASlopeHighSignal, SignalType.PDI20TEMASlopeLow, SignalType.PDI20TEMASlopeHigh, 1, TEMASlopeWobble);

			checkSetIndicatorLow(Data.Trix7, SignalType.Trix7LowSignal, SignalType.Trix7HighSignal, SignalType.Trix7Low, SignalType.Trix7High);
			checkSetIndicatorHigh(Data.Trix7, SignalType.Trix7LowSignal, SignalType.Trix7HighSignal, SignalType.Trix7Low, SignalType.Trix7High);
			checkSetIndicatorLow(Data.Trix10, SignalType.Trix10LowSignal, SignalType.Trix10HighSignal, SignalType.Trix10Low, SignalType.Trix10High);
			checkSetIndicatorHigh(Data.Trix10, SignalType.Trix10LowSignal, SignalType.Trix10HighSignal, SignalType.Trix10Low, SignalType.Trix10High);
			checkSetIndicatorLow(Data.Trix15, SignalType.Trix15LowSignal, SignalType.Trix15HighSignal, SignalType.Trix15Low, SignalType.Trix15High, 1, Wobble0005);
			checkSetIndicatorHigh(Data.Trix15, SignalType.Trix15LowSignal, SignalType.Trix15HighSignal, SignalType.Trix15Low, SignalType.Trix15High);
			checkSetIndicatorLow(Data.Trix15, SignalType.Trix15W0005LowSignal, SignalType.Trix15W0005HighSignal, SignalType.Trix15W0005Low, SignalType.Trix15W0005High, 1, Wobble0005);
			checkSetIndicatorHigh(Data.Trix15, SignalType.Trix15W0005LowSignal, SignalType.Trix15W0005HighSignal, SignalType.Trix15W0005Low, SignalType.Trix15W0005High);
			checkSetIndicatorLow(Data.Trix25, SignalType.Trix25LowSignal, SignalType.Trix25HighSignal, SignalType.Trix25Low, SignalType.Trix25High);
			checkSetIndicatorHigh(Data.Trix25, SignalType.Trix25LowSignal, SignalType.Trix25HighSignal, SignalType.Trix25Low, SignalType.Trix25High);
			checkSetIndicatorLow(Data.Trix25, SignalType.Trix25W0005LowSignal, SignalType.Trix25W0005HighSignal, SignalType.Trix25W0005Low, SignalType.Trix25W0005High, 1, Wobble0005);
			checkSetIndicatorHigh(Data.Trix25, SignalType.Trix25W0005LowSignal, SignalType.Trix25W0005HighSignal, SignalType.Trix25W0005Low, SignalType.Trix25W0005High);
			checkSetIndicatorLow(Data.Trix25, SignalType.Trix25W0008LowSignal, SignalType.Trix25W0008HighSignal, SignalType.Trix25W0008Low, SignalType.Trix25W0008High, 1, Wobble0008);
			checkSetIndicatorHigh(Data.Trix25, SignalType.Trix25W0008LowSignal, SignalType.Trix25W0008HighSignal, SignalType.Trix25W0008Low, SignalType.Trix25W0008High);

			checkSetIndicatorLow(Data.EFI4, SignalType.EFI4LowSignal, SignalType.EFI4HighSignal, SignalType.EFI4Low, SignalType.EFI4High);
			checkSetIndicatorHigh(Data.EFI4, SignalType.EFI4LowSignal, SignalType.EFI4HighSignal, SignalType.EFI4Low, SignalType.EFI4High);
			checkSetIndicatorLow(Data.T3, SignalType.T3LowSignal, SignalType.T3HighSignal, SignalType.T3Low, SignalType.T3High);
			checkSetIndicatorHigh(Data.T3, SignalType.T3LowSignal, SignalType.T3HighSignal, SignalType.T3Low, SignalType.T3High);
			checkSetIndicatorLow(Data.SVI4, SignalType.SVI4LowSignal, SignalType.SVI4HighSignal, SignalType.SVI4Low, SignalType.SVI4High);
			checkSetIndicatorHigh(Data.SVI4, SignalType.SVI4LowSignal, SignalType.SVI4HighSignal, SignalType.SVI4Low, SignalType.SVI4High);
			checkSetIndicatorLow(Data.PDI20T2, SignalType.PDI20T2LowSignal, SignalType.PDI20T2HighSignal, SignalType.PDI20T2Low, SignalType.PDI20T2High, 1, DIWobble); //
			checkSetIndicatorHigh(Data.PDI20T2, SignalType.PDI20T2LowSignal, SignalType.PDI20T2HighSignal, SignalType.PDI20T2Low, SignalType.PDI20T2High, 1, DIWobble); //
			checkSetIndicatorLow(Data.MDI20T2, SignalType.MDI20T2LowSignal, SignalType.MDI20T2HighSignal, SignalType.MDI20T2Low, SignalType.MDI20T2High, 1, DIWobble); //
			checkSetIndicatorHigh(Data.MDI20T2, SignalType.MDI20T2LowSignal, SignalType.MDI20T2HighSignal, SignalType.MDI20T2Low, SignalType.MDI20T2High, 1, DIWobble); //
			checkSetIndicatorLow(Data.PDI20T3, SignalType.PDI20T3LowSignal, SignalType.PDI20T3HighSignal, SignalType.PDI20T3Low, SignalType.PDI20T3High, 1); //
			checkSetIndicatorHigh(Data.PDI20T3, SignalType.PDI20T3LowSignal, SignalType.PDI20T3HighSignal, SignalType.PDI20T3Low, SignalType.PDI20T3High, 1); //
			checkSetIndicatorLow(Data.MDI20T3, SignalType.MDI20T3LowSignal, SignalType.MDI20T3HighSignal, SignalType.MDI20T3Low, SignalType.MDI20T3High, 1); //
			checkSetIndicatorHigh(Data.MDI20T3, SignalType.MDI20T3LowSignal, SignalType.MDI20T3HighSignal, SignalType.MDI20T3Low, SignalType.MDI20T3High, 1); //

			checkSetIndicatorTrackBelow(Data.PlusDI20, Data.MinusDI20, SignalType.MDI20OverPDI20, SignalType.PI20OverMDI20);
			checkSetIndicatorTrackBelow(Data.MinusDI20, Data.PlusDI20, SignalType.PI20OverMDI20, SignalType.MDI20OverPDI20);
			checkSetIndicatorTrackBelow(Data.PDI20T2, Data.MDI20T2, SignalType.MDI20T2OverPDI20T2, SignalType.PI20T2OverMDI20T2);
			checkSetIndicatorTrackBelow(Data.MDI20T2, Data.PDI20T2, SignalType.PI20T2OverMDI20T2, SignalType.MDI20T2OverPDI20T2);
			checkSetIndicatorTrackBelow(Data.PDI20T3, Data.MDI20T3, SignalType.MDI20T3OverPDI20T3, SignalType.PI20T3OverMDI20T3);
			checkSetIndicatorTrackBelow(Data.MDI20T3, Data.PDI20T3, SignalType.PI20T3OverMDI20T3, SignalType.MDI20T3OverPDI20T3);
			checkSetIndicatorTrackBelow(Data.Trix10, Data.Trix15, SignalType.Trix10XUnder15, SignalType.Trix10XOver15);
			checkSetIndicatorTrackBelow(Data.Trix15, Data.Trix10, SignalType.Trix10XOver15, SignalType.Trix10XUnder15);
			checkSetIndicatorTrackBelow(Data.Trix7, Data.Trix15, SignalType.Trix7XUnder15, SignalType.Trix7XOver15);
			checkSetIndicatorTrackBelow(Data.Trix15, Data.Trix7, SignalType.Trix7XOver15, SignalType.Trix7XUnder15);
			checkSetIndicatorTrackBelow(Data.Trix10, Data.Trix25, SignalType.Trix10XUnder25, SignalType.Trix10XOver25);
			checkSetIndicatorTrackBelow(Data.Trix25, Data.Trix10, SignalType.Trix10XOver25, SignalType.Trix10XUnder25);
			if (CurrentState.Status[es.StatusType.IsDIInverted] && deltaExceedsThreshold(Data.MinusDI20, Data.PlusDI20, 7)) { DurationMonitor[DType.DIInvertedDelta7]++; }
			if (!CurrentState.Status[es.StatusType.IsDIInverted] && DurationMonitor[DType.DIInvertedDelta7] > 0) { DurationMonitor[DType.DIInvertedDelta7] = 0; }
			if (CurrentState.Status[es.StatusType.IsDIPositive]) { DurationMonitor[DType.DIPositive]++; } else if (DurationMonitor[DType.DIPositive] > 0) { DurationMonitor[DType.DIPositive] = 0; }
			if (CurrentState.Status[es.StatusType.IsDIPositive] && deltaExceedsThreshold(Data.PlusDI20, Data.MinusDI20, 7)) { DurationMonitor[DType.DIPositiveDelta7]++; }
			if (!CurrentState.Status[es.StatusType.IsDIPositive] && DurationMonitor[DType.DIPositiveDelta7] > 0) { DurationMonitor[DType.DIPositiveDelta7] = 0; }
			if (CurrentState.Status[es.StatusType.IsDIPositive] && deltaExceedsThreshold(Data.PlusDI20, Data.MinusDI20, 2)) { DurationMonitor[DType.DIPositiveDelta2]++; }
			if (!CurrentState.Status[es.StatusType.IsDIPositive] && DurationMonitor[DType.DIPositiveDelta2] > 0) { DurationMonitor[DType.DIPositiveDelta2] = 0; }
			if (CurrentState.Status[es.StatusType.IsDIPositive] && deltaExceedsThreshold(Data.PlusDI20, Data.MinusDI20, 5)) { DurationMonitor[DType.DIPositiveDelta5]++; }
			if (!CurrentState.Status[es.StatusType.IsDIPositive] && DurationMonitor[DType.DIPositiveDelta5] > 0) { DurationMonitor[DType.DIPositiveDelta5] = 0; }

			TickMonitor[SignalType.Trix10LowGR4] = (TickMonitor[SignalType.Trix10XOver15] > TickMonitor[SignalType.Trix10LowGR4]) ? TickMonitor[SignalType.Trix10Low] : TickMonitor[SignalType.Trix10LowGR4];
			ValueMonitor[VType.Trix7UpSlope] = quantSlope(Data.Trix7, Ticks - TickMonitor[SignalType.Trix7Low]);
			ValueMonitor[VType.PriceAtTrix7Low] = (TickMonitor[SignalType.Trix7Low] >= 0) ? Data.IntraDayQuotes[TickMonitor[SignalType.Trix7Low]].Close : 0D;
			ValueMonitor[VType.VolumeAtTrix7Low] = (TickMonitor[SignalType.Trix7Low] >= 0) ? Data.IntraDayQuotes[TickMonitor[SignalType.Trix7Low]].Volume : 0D;
			ValueMonitor[VType.PDIMeanDelta] = (Data.PDI20T3.Tuples[ticks].QDatum.Value - DIMean);
			ValueMonitor[VType.MDIMeanDelta] = (DIMean - Data.MDI20T3.Tuples[ticks].QDatum.Value);
			ValueMonitor[VType.PDIMDIMeanFactor] = (ValueMonitor[VType.PDIMeanDelta] / ValueMonitor[VType.MDIMeanDelta]);
			ValueMonitor[VType.MDIPDIMeanFactor] = (ValueMonitor[VType.MDIMeanDelta] / ValueMonitor[VType.PDIMeanDelta]);
			ValueMonitor[VType.PDIMDIMeanFactorRatio] = (ValueMonitor[VType.PDIMDIMeanFactor] / ValueMonitor[VType.MDIPDIMeanFactor]);
			ValueMonitor[VType.MDIPDIMeanFactorRatio] = (ValueMonitor[VType.MDIPDIMeanFactor] / ValueMonitor[VType.PDIMDIMeanFactor]);

			ValueMonitor[VType.PDIMDIDelta] = (Data.PDI20T3.Tuples[ticks].QDatum.Value - Data.MDI20T3.Tuples[ticks].QDatum.Value);
			ValueMonitor[VType.MDIPDIDelta] = (Data.MDI20T3.Tuples[ticks].QDatum.Value - Data.PDI20T3.Tuples[ticks].QDatum.Value);
			ValueMonitor[VType.Trix7Delta] = (TickMonitor[SignalType.Trix7Low] >= 0) ? (Data.Trix7.Tuples[ticks].QDatum.Value - Data.Trix7.Tuples[TickMonitor[SignalType.Trix7Low]].QDatum.Value) : 0D;
		}

		public void EvaluateState(int ticks)
		{
			this.Ticks = ticks;
			this.CurrentDT = Data.IntraDayQuotes[ticks].Time;
			this.CurrentTime = this.CurrentDT.TimeOfDay;
			//Evaluate Time States
			if (CurrentState.Status[es.StatusType.IsBefore1030TBndry] && !CurrentState.Status[es.StatusType.IsAfter1000TBndry]) { CurrentState.Status[es.StatusType.IsAfter1000TBndry] = (CurrentTime >= Cache.TimeBoundary[Boundary.T1000]); }
			if (CurrentState.Status[es.StatusType.IsBefore1000TBndry] && CurrentState.Status[es.StatusType.IsAfter1000TBndry]) { CurrentState.Status[es.StatusType.IsBefore1000TBndry] = false; }
			if (CurrentState.Status[es.StatusType.IsBefore0945TBndry]) { CurrentState.Status[es.StatusType.IsBefore0945TBndry] = (CurrentTime <= Cache.TimeBoundary[Boundary.T0945]); }
			if (CurrentState.Status[es.StatusType.IsBefore1030TBndry]) { CurrentState.Status[es.StatusType.IsBefore1030TBndry] = (CurrentTime < Cache.TimeBoundary[Boundary.T1030]); }
			if (!CurrentState.Status[es.StatusType.IsBefore1030TBndry] && !CurrentState.Status[es.StatusType.IsAfter1030TBndry]) { CurrentState.Status[es.StatusType.IsAfter1030TBndry] = (CurrentTime >= Cache.TimeBoundary[Boundary.T1030]); }
			if (CurrentState.Status[es.StatusType.IsBefore1030TBndry]) { CurrentState.Status[es.StatusType.IsPDI20BelowThreshold] = isValueBelowThreshold(Data.PDI20T2, 25); }
			if (CurrentState.Status[es.StatusType.IsAfter1030TBndry]) { CurrentState.Status[es.StatusType.IsPDI20BelowThreshold] = isValueBelowThreshold(Data.PDI20T2, 25); }
			if (CurrentState.Status[es.StatusType.IsAfter1030TBndry]) { CurrentState.Status[es.StatusType.IsBefore1400TBndry] = (CurrentTime < Cache.TimeBoundary[Boundary.T1400]); }
			if (CurrentState.Status[es.StatusType.IsAfter1030TBndry]) { CurrentState.Status[es.StatusType.IsBefore1500TBndry] = (CurrentTime <= Cache.TimeBoundary[Boundary.T1500]); }
			if (CurrentState.Status[es.StatusType.IsAfter1030TBndry]) { CurrentState.Status[es.StatusType.IsBefore1530TBndry] = (CurrentTime <= Cache.TimeBoundary[Boundary.T1530]); }
			if (CurrentState.Status[es.StatusType.IsAfter1000TBndry]) { CurrentState.Status[es.StatusType.IsBtwn1000And1100] = (CurrentTime >= Cache.TimeBoundary[Boundary.T1000] && CurrentTime < Cache.TimeBoundary[Boundary.T1100]); }
			if (CurrentState.Status[es.StatusType.IsAfter1030TBndry]) { CurrentState.Status[es.StatusType.IsBtwn1030And1100] = (CurrentTime >= Cache.TimeBoundary[Boundary.T1030] && CurrentTime < Cache.TimeBoundary[Boundary.T1100]); }
			if (CurrentState.Status[es.StatusType.IsAfter1030TBndry]) { CurrentState.Status[es.StatusType.IsBtwn1400And1500] = (CurrentTime >= Cache.TimeBoundary[Boundary.T1400] && CurrentTime < Cache.TimeBoundary[Boundary.T1500]); }
			if (CurrentState.Status[es.StatusType.IsAfter1030TBndry]) { CurrentState.Status[es.StatusType.IsBtwn1500And1550] = (CurrentTime >= Cache.TimeBoundary[Boundary.T1500] && CurrentTime < Cache.TimeBoundary[Boundary.T1550]); }
			if (CurrentState.Status[es.StatusType.IsAfter1030TBndry] && !CurrentState.Status[es.StatusType.IsAfter1550TBndry]) { CurrentState.Status[es.StatusType.IsAfter1550TBndry] = (CurrentTime >= Cache.TimeBoundary[Boundary.T1550]); }
			if (CurrentState.Status[es.StatusType.IsAfter1030TBndry] && CurrentState.Status[es.StatusType.IsBefore1400TBndry])
			{ CurrentState.Status[es.StatusType.IsWithinMiddayBlackout] = (CurrentTime >= Cache.TimeBoundary[Boundary.T1240] && CurrentTime <= Cache.TimeBoundary[Boundary.T1330]); }
			checkSetCurrentContext(ticks); //Establish current quant event context
			CurrentState.Status[es.StatusType.IsDIInverted] = isQuantCompareGT(Data.MinusDI20, Data.PlusDI20);
			CurrentState.Status[es.StatusType.IsDIPositive] = isQuantCompareGT(Data.PlusDI20, Data.MinusDI20);
			CurrentState.Status[es.StatusType.IsDIT2Inverted] = isQuantCompareGT(Data.MDI20T2, Data.PDI20T2);
			CurrentState.Status[es.StatusType.IsDIT2Positive] = isQuantCompareGT(Data.PDI20T2, Data.MDI20T2);
			CurrentState.Status[es.StatusType.IsPDI20T2XOverMDI20T2] = isIndicatorHighTickCompareGT(SignalType.PI20T2OverMDI20T2, SignalType.MDI20T2OverPDI20T2);
			CurrentState.Status[es.StatusType.IsPDI20T3XOverMDI20T3] = isIndicatorHighTickCompareGT(SignalType.PI20T3OverMDI20T3, SignalType.MDI20T3OverPDI20T3);
			//Evaluate Trix States
			CurrentState.Status[es.StatusType.Trix10Under15] = isQuantCompareLT(Data.Trix10, Data.Trix15);
			CurrentState.Status[es.StatusType.Trix10Under25] = isQuantCompareLT(Data.Trix10, Data.Trix25);
			CurrentState.Status[es.StatusType.Trix15Under25] = isQuantCompareLT(Data.Trix15, Data.Trix25);
			CurrentState.Status[es.StatusType.Trix7Under10] = isQuantCompareLT(Data.Trix7, Data.Trix10);
			CurrentState.Status[es.StatusType.Trix7Under15] = isQuantCompareLT(Data.Trix7, Data.Trix15);
			CurrentState.Status[es.StatusType.Trix7Under25] = isQuantCompareLT(Data.Trix7, Data.Trix25);
			CurrentState.Status[es.StatusType.IsPDI20SlopeBelowMDI20Slope] = isQuantCompareLT(Data.PDI20TEMASlope, Data.MDI20TEMASlope);
			CurrentState.Status[es.StatusType.IsPDI20SlopeAboveMDI20Slope] = isQuantCompareGT(Data.PDI20TEMASlope, Data.MDI20TEMASlope);
			CurrentState.Status[es.StatusType.Trix7Over25] = isQuantCompareGT(Data.Trix7, Data.Trix25);
			CurrentState.Status[es.StatusType.Trix25Over7] = isQuantCompareGT(Data.Trix25, Data.Trix7);
			CurrentState.Status[es.StatusType.Trix7Over10] = isQuantCompareGT(Data.Trix7, Data.Trix10);
			CurrentState.Status[es.StatusType.Trix7Over15] = isQuantCompareGT(Data.Trix7, Data.Trix15);
			CurrentState.Status[es.StatusType.Trix10Over15] = isQuantCompareGT(Data.Trix10, Data.Trix15);
			CurrentState.Status[es.StatusType.Trix10Over25] = isQuantCompareGT(Data.Trix10, Data.Trix25);
			CurrentState.Status[es.StatusType.Trix25_7OverDelta] = deltaExceedsThreshold(Data.Trix25, Data.Trix7, TrixDelta);
			CurrentState.Status[es.StatusType.Trix7UptrendLB1] = isBackTrackTrendUp(Data.Trix7, 1);
			CurrentState.Status[es.StatusType.Trix7UptrendLB2] = isBackTrackTrendUp(Data.Trix7, 2);
			CurrentState.Status[es.StatusType.Trix7DowntrendLB2] = isBackTrackTrendDown(Data.Trix7, 2);
			CurrentState.Status[es.StatusType.Trix10DowntrendLB2] = isBackTrackTrendDown(Data.Trix10, 2);
			CurrentState.Status[es.StatusType.Trix10UptrendLB2] = isBackTrackTrendUp(Data.Trix10, 2);
			CurrentState.Status[es.StatusType.Trix15UptrendLB2] = isBackTrackTrendUp(Data.Trix15, 2);
			//CurrentState.Status[es.Type.Trix25DowntrendLB30] = isBackTrackTrendDown(Data.Trix25, 30);
			//CurrentState.Status[es.Type.IsTrix7x2Trix15] = (Math.Abs(Data.Trix7.Tuples[ticks].QDatum.Value) >= Math.Abs(Data.Trix15.Tuples[ticks].QDatum.Value)*2);
			CurrentState.Status[es.StatusType.IsTrix7AfterPeak] = isIndicatorHighTickCompareGT(SignalType.Trix7Low, SignalType.Trix7High);
			CurrentState.Status[es.StatusType.IsTrix7AfterLow] = isIndicatorLowTickCompareGT(SignalType.Trix7Low, SignalType.Trix7High);
			CurrentState.Status[es.StatusType.IsTrix10AfterPeak] = isIndicatorHighTickCompareGT(SignalType.Trix10Low, SignalType.Trix10High);
			CurrentState.Status[es.StatusType.IsTrix10AfterLow] = isIndicatorLowTickCompareGT(SignalType.Trix10Low, SignalType.Trix10High);
			CurrentState.Status[es.StatusType.IsTrix10AfterPeak] = isIndicatorHighTickCompareGT(SignalType.Trix10Low, SignalType.Trix10High);
			CurrentState.Status[es.StatusType.IsTrix15AfterLow] = isIndicatorLowTickCompareGT(SignalType.Trix15Low, SignalType.Trix15High);
			CurrentState.Status[es.StatusType.IsTrix15AfterPeak] = isIndicatorHighTickCompareGT(SignalType.Trix15Low, SignalType.Trix15High);
			CurrentState.Status[es.StatusType.IsTrix15W0005AfterLow] = isIndicatorLowTickCompareGT(SignalType.Trix15W0005Low, SignalType.Trix15W0005High);
			CurrentState.Status[es.StatusType.IsTrix15W0005AfterPeak] = isIndicatorHighTickCompareGT(SignalType.Trix15W0005Low, SignalType.Trix15W0005High);
			CurrentState.Status[es.StatusType.IsTrix25W0005AfterLow] = isIndicatorLowTickCompareGT(SignalType.Trix25W0005Low, SignalType.Trix25W0005High);
			CurrentState.Status[es.StatusType.IsTrix25W0005AfterPeak] = isIndicatorHighTickCompareGT(SignalType.Trix25W0005Low, SignalType.Trix25W0005High);
			CurrentState.Status[es.StatusType.IsTrix25W0008AfterLow] = isIndicatorLowTickCompareGT(SignalType.Trix25W0008Low, SignalType.Trix25W0008High);
			CurrentState.Status[es.StatusType.IsTrix25W0008AfterPeak] = isIndicatorHighTickCompareGT(SignalType.Trix25W0008Low, SignalType.Trix25W0008High);
			CurrentState.Status[es.StatusType.IsTrix25AfterLow] = isIndicatorLowTickCompareGT(SignalType.Trix25Low, SignalType.Trix25High);
			CurrentState.Status[es.StatusType.IsTrix25AfterPeak] = isIndicatorHighTickCompareGT(SignalType.Trix25Low, SignalType.Trix25High);
			CurrentState.Status[es.StatusType.IsMDI20TEMASlopeAfterLow] = isIndicatorLowTickCompareGT(SignalType.MDI20TEMASlopeLow, SignalType.MDI20TEMASlopeHigh);
			CurrentState.Status[es.StatusType.IsMDI20TEMASlopeAfterHigh] = isIndicatorHighTickCompareGT(SignalType.MDI20TEMASlopeLow, SignalType.MDI20TEMASlopeHigh);
			CurrentState.Status[es.StatusType.IsPDI20TEMASlopeAfterLow] = isIndicatorLowTickCompareGT(SignalType.PDI20TEMASlopeLow, SignalType.PDI20TEMASlopeHigh);
			CurrentState.Status[es.StatusType.IsPDI20TEMASlopeAfterHigh] = isIndicatorHighTickCompareGT(SignalType.PDI20TEMASlopeLow, SignalType.PDI20TEMASlopeHigh);

			CurrentState.Status[es.StatusType.IsT3AfterLow] = isIndicatorLowTickCompareGT(SignalType.T3Low, SignalType.T3High);
			CurrentState.Status[es.StatusType.IsT3AfterPeak] = isIndicatorHighTickCompareGT(SignalType.T3Low, SignalType.T3High);
			CurrentState.Status[es.StatusType.IsEFI4AfterLow] = isIndicatorLowTickCompareGT(SignalType.EFI4Low, SignalType.EFI4High);
			CurrentState.Status[es.StatusType.IsEFI4AfterPeak] = isIndicatorHighTickCompareGT(SignalType.EFI4Low, SignalType.EFI4High);
			CurrentState.Status[es.StatusType.IsEFI4BelowBuyLine] = (Data.EFI4.Tuples[ticks].QDatum < 0);
			CurrentState.Status[es.StatusType.IsEFI4AboveSellLine] = (Data.EFI4.Tuples[ticks].QDatum > 100);
			CurrentState.Status[es.StatusType.IsSVI4AfterLow] = isIndicatorLowTickCompareGT(SignalType.SVI4Low, SignalType.SVI4High);
			CurrentState.Status[es.StatusType.IsSVI4AfterPeak] = isIndicatorHighTickCompareGT(SignalType.SVI4Low, SignalType.SVI4High);
			CurrentState.Status[es.StatusType.IsSVI4BelowBuyLine] = (Data.SVI4.Tuples[ticks].QDatum < 0);
			CurrentState.Status[es.StatusType.IsSVI4AboveSellLine] = (Data.SVI4.Tuples[ticks].QDatum > 200);
			CurrentState.Status[es.StatusType.IsMDI20T2AfterLow] = isIndicatorLowTickCompareGT(SignalType.MDI20T2Low, SignalType.MDI20T2High);
			CurrentState.Status[es.StatusType.IsMDI20T2AfterPeak] = isIndicatorHighTickCompareGT(SignalType.MDI20T2Low, SignalType.MDI20T2High);
			CurrentState.Status[es.StatusType.IsPDI20T2AfterLow] = isIndicatorLowTickCompareGT(SignalType.PDI20T2Low, SignalType.PDI20T2High);
			CurrentState.Status[es.StatusType.IsPDI20T2AfterPeak] = isIndicatorHighTickCompareGT(SignalType.PDI20T2Low, SignalType.PDI20T2High);
			CurrentState.Status[es.StatusType.IsMDI20T3AfterLow] = isIndicatorLowTickCompareGT(SignalType.MDI20T3Low, SignalType.MDI20T3High);
			CurrentState.Status[es.StatusType.IsMDI20T3AfterPeak] = isIndicatorHighTickCompareGT(SignalType.MDI20T3Low, SignalType.MDI20T3High);
			CurrentState.Status[es.StatusType.IsPDI20T3AfterLow] = isIndicatorLowTickCompareGT(SignalType.PDI20T3Low, SignalType.PDI20T3High);
			CurrentState.Status[es.StatusType.IsPDI20T3AfterPeak] = isIndicatorHighTickCompareGT(SignalType.PDI20T3Low, SignalType.PDI20T3High);
			CurrentState.Status[es.StatusType.IsPDI20BelowThreshold] = isValueBelowThreshold(Data.PDI20T2, 25);
			CurrentState.Status[es.StatusType.IsPDI20AboveThreshold] = isValueAboveThreshold(Data.PDI20T3, 40);
			CurrentState.Status[es.StatusType.IsMDI20AboveThreshold] = isValueAboveThreshold(Data.MDI20T3, 50);
			int T3LowLagThrehold = (TickMonitor[SignalType.T3Low] == 0) ? 8 : 5;
			if (CurrentState.Status[es.StatusType.IsT3AfterLow] && isTickDeltaAboveThreshold(SignalType.T3Low, T3LowLagThrehold))
			{ CurrentState.Status[es.StatusType.IsWithinT3BuyRange] = false; }
			else { CurrentState.Status[es.StatusType.IsWithinT3BuyRange] = true; }

			CurrentState.Status[es.StatusType.IsTrix10AfterPeakGTTrix15] = (CurrentState.Status[es.StatusType.IsTrix10AfterPeak] && CurrentState.Status[es.StatusType.Trix10Over15]);
			CurrentState.Status[es.StatusType.IsTicksAfterTrix10LowLE5] = (CurrentState.Status[es.StatusType.IsTrix10AfterLow] && isTickDeltaBelowThreshold(SignalType.Trix10Low, 5));
			CurrentState.Status[es.StatusType.IsTicksAfterTrix10LowGE1] = (CurrentState.Status[es.StatusType.IsTrix10AfterLow] && isTickDeltaAboveThreshold(SignalType.Trix10Low, 1));
			CurrentState.Status[es.StatusType.IsTicksAfterTrix7LowLE5] = (CurrentState.Status[es.StatusType.IsTrix7AfterLow] && isTickDeltaBelowThreshold(SignalType.Trix7Low, 5));
			CurrentState.Status[es.StatusType.IsTicksAfterTrix7LowGE1] = (CurrentState.Status[es.StatusType.IsTrix7AfterLow] && isTickDeltaAboveThreshold(SignalType.Trix7Low, 1));
			CurrentState.Status[es.StatusType.Is1TickAfterTrix7Peak] = (CurrentState.Status[es.StatusType.IsTrix7AfterPeak] && isTickDeltaAtMark(SignalType.Trix7High, 1));
			CurrentState.Status[es.StatusType.Is2TicksAfterTrix7Peak] = (CurrentState.Status[es.StatusType.IsTrix7AfterPeak] && isTickDeltaAtMark(SignalType.Trix7High, 2));
			CurrentState.Status[es.StatusType.IsTicksAfterTrix7PeakGE1] = (CurrentState.Status[es.StatusType.IsTrix7AfterPeak] && isTickDeltaAboveThreshold(SignalType.Trix7High, 1));
			CurrentState.Status[es.StatusType.IsTicksAfterTrix7PeakGE2] = (CurrentState.Status[es.StatusType.IsTrix7AfterPeak] && isTickDeltaAboveThreshold(SignalType.Trix7High, 2));
			CurrentState.Status[es.StatusType.Is2TicksAfterTrix15Low] = (CurrentState.Status[es.StatusType.IsTrix15AfterLow] && isTickDeltaAtMark(SignalType.Trix15Low, 2));
			CurrentState.Status[es.StatusType.Is1TickAfterTrix10Low] = (CurrentState.Status[es.StatusType.IsTrix10AfterLow] && isTickDeltaAtMark(SignalType.Trix10Low, 1));
			CurrentState.Status[es.StatusType.Is2TicksAfterTrix10Low] = (CurrentState.Status[es.StatusType.IsTrix10AfterLow] && isTickDeltaAtMark(SignalType.Trix10Low, 2));
			CurrentState.Status[es.StatusType.IsTrix7SlopeSteepEnough] = isQuantSlopeExceedThreshold(Data.Trix7, Ticks - TickMonitor[SignalType.Trix7Low], Operator.GT, .004D);
			CurrentState.Status[es.StatusType.IsTrix10SlopeSteepEnough] = isQuantSlopeExceedThreshold(Data.Trix10, Ticks - TickMonitor[SignalType.Trix10Low], Operator.GT, .004D);
			CurrentState.Status[es.StatusType.IsPDI20SteepEnough] = isQuantSlopeExceedThreshold(Data.PDI20T3, Ticks - TickMonitor[SignalType.PDI20T3Low], Operator.GT, .4D);
			CurrentState.Status[es.StatusType.IsTrix7SteepDecline] = isQuantSlopeExceedThreshold(Data.Trix7, Ticks - TickMonitor[SignalType.Trix7High], Operator.LT, -.0033D);
			CurrentState.Status[es.StatusType.Trix10Under25AtPeakTrix7] = (CurrentState.Status[es.StatusType.IsTrix7AfterPeak] && isQuantCompareLT(Data.Trix10, Data.Trix25, Ticks - TickMonitor[SignalType.Trix7High]));
			CurrentState.Status[es.StatusType.Trix7Over25AtPeakTrix7] = (CurrentState.Status[es.StatusType.IsTrix7AfterPeak] && isQuantCompareGT(Data.Trix7, Data.Trix25, Ticks - TickMonitor[SignalType.Trix7High]));
			CurrentState.Status[es.StatusType.Trix7Under25AtLowTrix10] = (CurrentState.Status[es.StatusType.IsTrix10AfterLow] && isQuantCompareLT(Data.Trix7, Data.Trix25, Ticks - TickMonitor[SignalType.Trix10Low]));
			CurrentState.Status[es.StatusType.Trix10Under25AtLowTrix10] = (CurrentState.Status[es.StatusType.IsTrix10AfterLow] && isQuantCompareLT(Data.Trix10, Data.Trix25, Ticks - TickMonitor[SignalType.Trix10Low]));
			CurrentState.Status[es.StatusType.Trix10Under15AtLowTrix10] = (CurrentState.Status[es.StatusType.IsTrix10AfterLow] && isQuantCompareLT(Data.Trix10, Data.Trix15, Ticks - TickMonitor[SignalType.Trix10Low]));
			CurrentState.Status[es.StatusType.Trix7Under15AtLowTrix10] = (CurrentState.Status[es.StatusType.IsTrix10AfterLow] && isQuantCompareLT(Data.Trix7, Data.Trix15, Ticks - TickMonitor[SignalType.Trix10Low]));
			CurrentState.Status[es.StatusType.Trix10Under15SinceClose_GR4] = (TickMonitor[SignalType.Trix10LowGR4] < TickMonitor[SignalType.Trix10High]);
			if (!CurrentState.Status[es.StatusType.Trix10DowntrendLB30_GR5])
			{
				int lag = (TickMonitor[SignalType.Trix10High] > 0) ? (int)CurrentDT.Subtract(Data.IntraDayQuotes[TickMonitor[SignalType.Trix10High]].Time).TotalMinutes : 0;
				CurrentState.Status[es.StatusType.Trix10DowntrendLB30_GR5] = (CurrentState.Status[es.StatusType.IsTrix10AfterPeak] && (lag >= (DurationMonitor[DType.GR7Duration])));
			}
			else if (CurrentState.Status[es.StatusType.Trix10DowntrendLB30_GR5] && CurrentState.Status[es.StatusType.IsTrix10AfterLow])
			{ CurrentState.Status[es.StatusType.Trix10DowntrendLB30_GR5] = false; }

			if (!CurrentState.Status[es.StatusType.Trix15DowntrendLB20_GR5])
			{
				int lag = (TickMonitor[SignalType.Trix15High] > 0) ? (int)CurrentDT.Subtract(Data.IntraDayQuotes[TickMonitor[SignalType.Trix15High]].Time).TotalMinutes : 0;
				CurrentState.Status[es.StatusType.Trix15DowntrendLB20_GR5] = (CurrentState.Status[es.StatusType.IsTrix15W0005AfterPeak] && (lag >= (DurationMonitor[DType.GR5Duration])));
			}
			else if (CurrentState.Status[es.StatusType.Trix15DowntrendLB20_GR5] && CurrentState.Status[es.StatusType.IsTrix15W0005AfterLow] &&
				(CurrentState.Status[es.StatusType.IsTrix10AfterLow] || (CurrentState.Status[es.StatusType.Trix7Over15] && CurrentState.Status[es.StatusType.Trix10Over15])))
			{ CurrentState.Status[es.StatusType.Trix15DowntrendLB20_GR5] = false; }

			if (CurrentState.Status[es.StatusType.IsAfter1030TBndry] && !CurrentState.Status[es.StatusType.Trix10XUnder25DurationGT30_GR6] && TickMonitor[SignalType.Trix10XUnder25] > TickMonitor[SignalType.Trix10XOver25] && CurrentState.Status[es.StatusType.IsTrix25W0005AfterPeak])
			{
				int lag = (TickMonitor[SignalType.Trix25W0005High] > 0) ? (int)CurrentDT.Subtract(Data.IntraDayQuotes[TickMonitor[SignalType.Trix25W0005High]].Time).TotalMinutes : 0;
				CurrentState.Status[es.StatusType.Trix10XUnder25DurationGT30_GR6] = (lag >= DurationMonitor[DType.GR6Duration]);
			}
			else if (CurrentState.Status[es.StatusType.Trix10XUnder25DurationGT30_GR6] && CurrentState.Status[es.StatusType.IsTrix25AfterLow] &&
				(CurrentState.Status[es.StatusType.IsTrix10AfterLow] || (CurrentState.Status[es.StatusType.Trix7UptrendLB2] && CurrentState.Status[es.StatusType.Trix10UptrendLB2] && CurrentState.Status[es.StatusType.Trix15UptrendLB2])))
			{ CurrentState.Status[es.StatusType.Trix10XUnder25DurationGT30_GR6] = false; }

			if (CurrentState.Status[es.StatusType.IsAfter1030TBndry] && !CurrentState.Status[es.StatusType.Trix25DowntrendLB60] && CurrentState.Status[es.StatusType.IsTrix25W0005AfterPeak])
			{
				int lag = (TickMonitor[SignalType.Trix25W0005High] > 0) ? (int)CurrentDT.Subtract(Data.IntraDayQuotes[TickMonitor[SignalType.Trix25W0005High]].Time).TotalMinutes : 0;
				CurrentState.Status[es.StatusType.Trix25DowntrendLB60] = (lag >= DurationMonitor[DType.Trix25Downtrend]);
			}
			else if (CurrentState.Status[es.StatusType.Trix25DowntrendLB60] && CurrentState.Status[es.StatusType.IsTrix25AfterLow] &&
				(CurrentState.Status[es.StatusType.IsTrix10AfterLow] || (CurrentState.Status[es.StatusType.Trix7UptrendLB2] && CurrentState.Status[es.StatusType.Trix10UptrendLB2] && CurrentState.Status[es.StatusType.Trix15UptrendLB2])))
			{ CurrentState.Status[es.StatusType.Trix25DowntrendLB60] = false; }

			if (CurrentState.Status[es.StatusType.IsAfter1030TBndry] && !CurrentState.Status[es.StatusType.Trix7XUnder15DurationGT30_GR6] && TickMonitor[SignalType.Trix7XUnder15] > TickMonitor[SignalType.Trix7XOver15] && CurrentState.Status[es.StatusType.IsTrix15W0005AfterPeak])
			{
				int lag = (TickMonitor[SignalType.Trix15W0005High] > 0) ? (int)CurrentDT.Subtract(Data.IntraDayQuotes[TickMonitor[SignalType.Trix15W0005High]].Time).TotalMinutes : 0;
				CurrentState.Status[es.StatusType.Trix7XUnder15DurationGT30_GR6] = (lag >= DurationMonitor[DType.GR6Duration]);
			}
			else if (CurrentState.Status[es.StatusType.Trix10XUnder25DurationGT30_GR6] && CurrentState.Status[es.StatusType.IsTrix25AfterLow] &&
				(CurrentState.Status[es.StatusType.IsTrix10AfterLow] || (CurrentState.Status[es.StatusType.Trix7UptrendLB2] && CurrentState.Status[es.StatusType.Trix10UptrendLB2] && CurrentState.Status[es.StatusType.Trix15UptrendLB2])))
			{ CurrentState.Status[es.StatusType.Trix10XUnder25DurationGT30_GR6] = false; }

			if (CurrentState.Status[es.StatusType.IsAfter1030TBndry] && !CurrentState.Status[es.StatusType.Trix25UptrendDurationGE30_GR7] && CurrentState.Status[es.StatusType.IsTrix10AfterLow] && CurrentState.Status[es.StatusType.IsTrix25W0008AfterLow])
			{
				int lag25W0008Low = (TickMonitor[SignalType.Trix25W0008Low] > 0) ? (int)CurrentDT.Subtract(Data.IntraDayQuotes[TickMonitor[SignalType.Trix25W0008Low]].Time).TotalMinutes : 0;
				int lag10Low25LowDelta = (TickMonitor[SignalType.Trix10Low] > 0) ? (int)Data.IntraDayQuotes[TickMonitor[SignalType.Trix10Low]].Time.Subtract(Data.IntraDayQuotes[TickMonitor[SignalType.Trix25W0008Low]].Time).TotalMinutes : 0;
				CurrentState.Status[es.StatusType.Trix25UptrendDurationGE30_GR7] = (lag25W0008Low >= DurationMonitor[DType.GR7Duration] && lag10Low25LowDelta >= DurationMonitor[DType.GR7Duration]);
			}
			else { CurrentState.Status[es.StatusType.Trix25UptrendDurationGE30_GR7] = false; }

			if (!CurrentState.Status[es.StatusType.IsDIInvertedDurationGT30_GR11] && CurrentState.Status[es.StatusType.IsDIInverted])
			{
				int lag = (TickMonitor[SignalType.MDI20OverPDI20] > 0) ? (int)CurrentDT.Subtract(Data.IntraDayQuotes[TickMonitor[SignalType.MDI20OverPDI20]].Time).TotalMinutes : 0;
				CurrentState.Status[es.StatusType.IsDIInvertedDurationGT30_GR11] = (lag > DurationMonitor[DType.GR11Duration]);
			}
			else if (CurrentState.Status[es.StatusType.IsDIInvertedDurationGT30_GR11] && (CurrentState.Status[es.StatusType.IsDIPositive] || CurrentState.Status[es.StatusType.Is2TicksAfterTrix15Low]))
			{ CurrentState.Status[es.StatusType.IsDIInvertedDurationGT30_GR11] = false; }

			//Evaluate Directional Indicator States
			CurrentState.Status[es.StatusType.IsDeltaPDI20_MDI20le1] = deltaBelowThreshold(Data.PlusDI20, Data.MinusDI20, 1);
			CurrentState.Status[es.StatusType.IsDeltaPDI20_MDI20gt2] = deltaExceedsThreshold(Data.PlusDI20, Data.MinusDI20, 2);
			CurrentState.Status[es.StatusType.IsDeltaPDI20_MDI20gt5] = deltaExceedsThreshold(Data.PlusDI20, Data.MinusDI20, 5);
			CurrentState.Status[es.StatusType.IsDeltaPDI20_MDI20gt10] = deltaExceedsThreshold(Data.PDI20T2, Data.MDI20T2, 10);
			CurrentState.Status[es.StatusType.IsDeltaMDI20_PDI20gt5] = deltaExceedsThreshold(Data.MinusDI20, Data.PlusDI20, 5);
			CurrentState.Status[es.StatusType.IsDeltaPDI20T2_MDI20T2le5] = deltaBelowThreshold(Data.PDI20T2, Data.MDI20T2, 5);
			CurrentState.Status[es.StatusType.IsDeltaPDI20T2_MDI20T2le9] = deltaBelowThreshold(Data.PDI20T2, Data.MDI20T2, 9);
			CurrentState.Status[es.StatusType.IsDeltaPDI20_MDI20le5] = deltaBelowThreshold(Data.PlusDI20, Data.MinusDI20, 5);
			CurrentState.Status[es.StatusType.IsDeltaMDI20_PDI20le5] = deltaBelowThreshold(Data.MinusDI20, Data.PlusDI20, 5);
			CurrentState.Status[es.StatusType.IsDIInvertedAtTrix10Low] = (CurrentState.Status[es.StatusType.IsTrix10AfterLow] && isQuantCompareGT(Data.MinusDI20, Data.PlusDI20, ticks - TickMonitor[SignalType.Trix10Low]));
			CurrentState.Status[es.StatusType.IsDIPositiveAtTrix10Low] = (CurrentState.Status[es.StatusType.IsTrix10AfterLow] && isQuantCompareGT(Data.PlusDI20, Data.MinusDI20, ticks - TickMonitor[SignalType.Trix10Low]));
			CurrentState.Status[es.StatusType.IsDIInvertedDelta7DurationGE2] = (DurationMonitor[DType.DIInvertedDelta7] >= 1);
			CurrentState.Status[es.StatusType.IsDIPositiveDelta7DurationGE2] = (DurationMonitor[DType.DIPositiveDelta7] >= 2);
			CurrentState.Status[es.StatusType.IsDIPositiveDelta2DurationGE2] = (DurationMonitor[DType.DIPositiveDelta2] >= 2);
			CurrentState.Status[es.StatusType.IsDIPositiveDelta5DurationGT0] = (DurationMonitor[DType.DIPositiveDelta5] > 0);
			CurrentState.Status[es.StatusType.IsDIPositiveDurationGT10] = (CurrentState.Status[es.StatusType.IsDIPositive] && isTickDeltaAboveThreshold(SignalType.PI20OverMDI20, 10));
			CurrentState.Status[es.StatusType.IsDIPositiveDurationGTLimit] = (CurrentState.Status[es.StatusType.IsDIPositive] && isTickDeltaAboveThreshold(SignalType.PI20OverMDI20, DurationMonitor[DType.PDI20DurationLimit]));
			CurrentState.Status[es.StatusType.IsPDI20gt30] = (Data.PlusDI20.Series[ticks].QDatum.Value > 30);
			CurrentState.Status[es.StatusType.IsSellPressureDeclining] = (!Data.MDI20SlopePeakTracker.Contains(-10) && Data.MDI20SlopePeakTracker[1] < Data.MDI20SlopePeakTracker[0]);
			CurrentState.Status[es.StatusType.IsPDI20SlopeUp] = (Data.PDI20TEMASlope.Tuples[ticks].QDatum.Value > 0D);
			CurrentState.Status[es.StatusType.IsPDI20SlopeBelowThreshold] = (Data.PDI20TEMASlope.Tuples[ticks].QDatum.Value < 1D);
			CurrentState.Status[es.StatusType.IsDIPositiveAboveThreshold] = (DurationMonitor[DType.DIPositive] >= 20);
			CurrentState.Status[es.StatusType.IsAboveStopLossThreshold] = (ticks > TickMonitor[SignalType.LastBuyTick] && TickMonitor[SignalType.LastBuyTick] > 0 && ((Data.IntraDayQuotes[Ticks].Close - Data.IntraDayQuotes[TickMonitor[SignalType.LastBuyTick]].Close) / Data.IntraDayQuotes[TickMonitor[SignalType.LastBuyTick]].Close) * 100 < StopLossPct);
			CurrentState.Status[es.StatusType.IsPriceAboveBackstop1Pct] = (ticks > TickMonitor[SignalType.LastBuyTick] && TickMonitor[SignalType.LastBuyTick] > 0 && (Data.IntraDayQuotes[ticks].Close / Data.IntraDayQuotes[TickMonitor[SignalType.LastBuyTick]].Close > 1.013D));
			CurrentState.Status[es.StatusType.IsBackstop1PctActivated] = (CurrentState.Status[es.StatusType.IsBackstop1PctActivated] || CurrentState.Status[es.StatusType.IsPriceAboveBackstop1Pct]);
			CurrentState.Status[es.StatusType.IsPriceAboveBackstop8Pct] = (ticks > TickMonitor[SignalType.LastBuyTick] && TickMonitor[SignalType.LastBuyTick] > 0 && (Data.IntraDayQuotes[ticks].Close / Data.IntraDayQuotes[TickMonitor[SignalType.LastBuyTick]].Close > 1.08D));
			CurrentState.Status[es.StatusType.IsBackstop8PctActivated] = (CurrentState.Status[es.StatusType.IsBackstop8PctActivated] || CurrentState.Status[es.StatusType.IsPriceAboveBackstop8Pct]);
			CurrentState.Status[es.StatusType.IsTrix7xDTrix15] = isXDIndicator(es.StatusType.IsTrix7xDTrix15, SignalType.Trix7xDTrix15, Data.Trix7.Tuples[ticks].QDatum.Value, Data.Trix15.Tuples[ticks].QDatum.Value, 1.3D);
			CurrentState.Status[es.StatusType.IsTrix7x2DTrix15] = isXDIndicator(es.StatusType.IsTrix7x2DTrix15, SignalType.Trix7x2DTrix15, Data.Trix7.Tuples[ticks].QDatum.Value, Data.Trix10.Tuples[ticks].QDatum.Value, 2D);
			//ValueMonitor[VType.Trix7Delta]
			CurrentState.Status[es.StatusType.IsTrix7RiseAboveThreshold] = (CurrentState.Status[es.StatusType.IsTrix7AfterLow] && deltaExceedsThreshold(Data.Trix7, SignalType.Trix7Low, .43D));

			double priceDelta = (CurrentState.Status[es.StatusType.IsTrix7AfterLow]) ? (Data.IntraDayQuotes[ticks].Close - ValueMonitor[VType.PriceAtTrix7Low]) : 0D;
			double volumeDelta = (CurrentState.Status[es.StatusType.IsTrix7AfterLow]) ? (Data.IntraDayQuotes[ticks].Volume - ValueMonitor[VType.VolumeAtTrix7Low]) : 0D;
			CurrentState.Status[es.StatusType.IsMDI20MeanDeltaBelowT1P1] = (ValueMonitor[VType.MDIMeanDelta] <= -3D);
			CurrentState.Status[es.StatusType.IsMDI20MeanDeltaAboveT2P1] = (ValueMonitor[VType.MDIMeanDelta] >= 17.5D); //or MDIMeanDelta > 17.5 (not necessary)
			CurrentState.Status[es.StatusType.IsVolumeDeltaAboveT1P1] = (CurrentState.Status[es.StatusType.IsTrix7AfterLow] && volumeDelta > -16000);
			CurrentState.Status[es.StatusType.IsPriceDeltaBelowT1P1] = (CurrentState.Status[es.StatusType.IsTrix7AfterLow] && (priceDelta <= 3.05)); //priceDelta >= .435 &&
			CurrentState.Status[es.StatusType.IsPDIMeanFactorAboveT1P1] = (ValueMonitor[VType.PDIMDIMeanFactor] > 3D);
			CurrentState.Status[es.StatusType.IsMDIMeanFactorAboveT1P1] = (ValueMonitor[VType.MDIPDIMeanFactor] > 1D);

			CurrentState.Status[es.StatusType.IsDIDeltaBelowT1P2] = (ValueMonitor[VType.PDIMDIDelta] < -17.5D);
			CurrentState.Status[es.StatusType.IsDIDeltaAboveT2P2] = (ValueMonitor[VType.PDIMDIDelta] > -8.5D);
			CurrentState.Status[es.StatusType.IsMDIPDIMeanDeltaAboveT1P2] = (ValueMonitor[VType.MDIMeanDelta] - ValueMonitor[VType.PDIMeanDelta] >= -1D);
			CurrentState.Status[es.StatusType.IsMDIPDIMeanDeltaBelowT2P2] = (ValueMonitor[VType.MDIMeanDelta] - ValueMonitor[VType.PDIMeanDelta] < -4.5D);
			CurrentState.Status[es.StatusType.IsPDI20T3AboveT1P2] = (Data.PDI20T3.Tuples[ticks].QDatum.Value >= 19D);
			CurrentState.Status[es.StatusType.IsPDI20T3BelowT2P2] = (Data.PDI20T3.Tuples[ticks].QDatum.Value < 14D);

			CurrentState.Status[es.StatusType.IsTrix7DeltaBelowT1P3] = (ValueMonitor[VType.Trix7Delta] < .0307D);
			CurrentState.Status[es.StatusType.IsTrix7DeltaAboveT2P3] = (ValueMonitor[VType.Trix7Delta] > .078D);
			CurrentState.Status[es.StatusType.IsPDI20T3AboveT1P3] = (Data.PDI20T3.Tuples[ticks].QDatum.Value >= 17.25D);
			CurrentState.Status[es.StatusType.IsEFIDeltaBelowT1P3] = (Data.EFI4.Tuples[ticks].QDatum.Value < -1550D);
			CurrentState.Status[es.StatusType.IsSVIWithinT1P3] = (Data.SVI4.Tuples[ticks].QDatum.Value >= -327D && Data.SVI4.Tuples[ticks].QDatum.Value <= -80D);
			CurrentState.Status[es.StatusType.IsDIDeltaBelowT1P3] = (ValueMonitor[VType.PDIMDIDelta] <= .75D);
			CurrentState.Status[es.StatusType.IsDIDeltaInvAboveT1P3] = (ValueMonitor[VType.MDIPDIDelta] > 4D);
			CurrentState.Status[es.StatusType.IsDIDeltaInvBelowT2P3] = (ValueMonitor[VType.MDIPDIDelta] < 3.5D);
			CurrentState.Status[es.StatusType.IsTrix7DeltaWithinT1P3] = (ValueMonitor[VType.Trix7Delta] >= .0825D && ValueMonitor[VType.Trix7Delta] <= .1045D);
			CurrentState.Status[es.StatusType.IsTrix7DeltaWithinT2P3] = (ValueMonitor[VType.Trix7Delta] >= .0087D && ValueMonitor[VType.Trix7Delta] <= .0137D);
			CurrentState.Status[es.StatusType.IsVolumeDeltaBelowT1P3] = (CurrentState.Status[es.StatusType.IsTrix7AfterLow] && volumeDelta < 15900);
			CurrentState.Status[es.StatusType.IsPriceDeltaBelowT1P3] = (CurrentState.Status[es.StatusType.IsTrix7AfterLow] && priceDelta <= .86D);
			CurrentState.Status[es.StatusType.IsMDI20MeanDeltaWithinT1P3] = (ValueMonitor[VType.MDIMeanDelta] >= -.21D && ValueMonitor[VType.MDIMeanDelta] <= .21D);

			CurrentState.Status[es.StatusType.IsDeltaPDI20_MDI20AboveThreshold] = CurrentState.Status[es.StatusType.IsDIT2Positive] && ((Data.PDI20T3.Tuples[ticks].QDatum.Value - Data.MDI20T3.Tuples[ticks].QDatum.Value) > 35D);
			CurrentState.Status[es.StatusType.IsDeltaMDI20_PDI20AboveThreshold] = CurrentState.Status[es.StatusType.IsDIT2Inverted] && ((Data.MDI20T3.Tuples[ticks].QDatum.Value - Data.PDI20T3.Tuples[ticks].QDatum.Value) > 20D);
		}

		#region Tools/Utilities

		private bool isXDIndicator(es.StatusType stateType, SignalType signalType, double quantTrack, double quantCompare, double factor = 1.5D)
		{
			bool evalStatus = false, currentStatus = CurrentState.Status[stateType];
			if (quantTrack > 0)
			{
				evalStatus = (quantCompare >= 0) ? (quantTrack > quantCompare * factor) : (quantTrack + Math.Abs(quantCompare) > Math.Abs(quantCompare) * factor);
			}
			else
			{
				evalStatus = (quantCompare >= 0) ? (Math.Abs(quantTrack) + quantCompare > quantCompare * factor) : (Math.Abs(quantTrack) > Math.Abs(quantCompare) * factor);
			}
			//if (currentStatus != evalStatus) { this.Signals.Add(new Signal(Ticks, Ticks, this.Data, signalType, quantTrack, false)); }
			return evalStatus;
		}

		private void checkSetIndicatorTrackBelow(Indicator quantLow, Indicator quantHigh, SignalType trackerLow, SignalType trackerHigh)
		{
			//if (CurrentState.Status[es.Type.IsBefore1030TBndry]) { return; }
			if (isQuantCompareLT(quantLow, quantHigh) && TickMonitor[trackerLow] <= TickMonitor[trackerHigh])
			{
				TickMonitor[trackerLow] = Ticks;
				//this.Signals.Add(new Signal(Ticks, TickMonitor[trackerLow], this.Data, trackerLow, quantLow.Series[TickMonitor[trackerLow]].QDatum.Value));
			}
		}

		private void checkSetIndicatorLow(Indicator quant, SignalType indicatorLowSignal, SignalType indicatorHighSignal, SignalType indicatorLow, SignalType indicatorHigh, int lookback = 1, double wobble = 0D)
		{
			int tickBack = 0;
			if (lookback > Ticks) { return; }
			if (isBackTrackLow(quant, lookback, out tickBack))
			{ TickMonitor[indicatorLowSignal] = Ticks - lookback - tickBack; }
			if (TickMonitor[indicatorLowSignal] > -1
				&& (TickMonitor[indicatorHigh] > TickMonitor[indicatorLow] || TickMonitor[indicatorHigh] == -1)
				&& (Math.Abs(quant.Series[Ticks].QDatum.Value - quant.Series[TickMonitor[indicatorLowSignal]].QDatum.Value) > wobble || wobble == 0D))
			{
				TickMonitor[indicatorLow] = TickMonitor[indicatorLowSignal];
				//this.Signals.Add(new Signal(Ticks, TickMonitor[indicatorLowSignal], this.Data, indicatorLow, quant.Series[TickMonitor[indicatorLowSignal]].QDatum.Value));
				TickMonitor[indicatorHighSignal] = TickMonitor[indicatorLowSignal] = -1;
			}
		}

		private void checkSetIndicatorHigh(Indicator quant, SignalType indicatorLowSignal, SignalType indicatorHighSignal, SignalType indicatorLow, SignalType indicatorHigh, int lookback = 1, double wobble = 0D)
		{
			if (lookback > Ticks) { return; }
			if (isBackTrackHigh(quant, lookback))
			{ TickMonitor[indicatorHighSignal] = Ticks - lookback; }
			if (TickMonitor[indicatorHighSignal] > -1
				&& (TickMonitor[indicatorLow] > TickMonitor[indicatorHigh] || TickMonitor[indicatorLow] == -1)
				&& (Math.Abs(quant.Series[Ticks].QDatum.Value - quant.Series[TickMonitor[indicatorHighSignal]].QDatum.Value) > wobble || wobble == 0D))
			{
				TickMonitor[indicatorHigh] = TickMonitor[indicatorHighSignal];
				//this.Signals.Add(new Signal(Ticks, TickMonitor[indicatorHighSignal], this.Data, indicatorHigh, quant.Series[TickMonitor[indicatorHighSignal]].QDatum.Value));
				TickMonitor[indicatorLowSignal] = TickMonitor[indicatorHighSignal] = -1;
			}
		}

		private void checkSetIndicatorHigh(Indicator quant, double[] quantTracker, SignalType indicatorLowSignal, SignalType indicatorHighSignal, SignalType indicatorLow, SignalType indicatorHigh, int lookback = 1, double wobble = 0D)
		{
			if (lookback > Ticks) { return; }
			int i;
			if (isBackTrackHigh(quant, lookback))
			{ TickMonitor[indicatorHighSignal] = Ticks - lookback; }
			if (TickMonitor[indicatorHighSignal] > -1
				&& (TickMonitor[indicatorLow] > TickMonitor[indicatorHigh] || TickMonitor[indicatorLow] == -1)
				&& (Math.Abs(quant.Series[Ticks].QDatum.Value - quant.Series[TickMonitor[indicatorHighSignal]].QDatum.Value) > wobble || wobble == 0D))
			{
				TickMonitor[indicatorHigh] = TickMonitor[indicatorHighSignal];
				//this.Signals.Add(new Signal(Ticks, TickMonitor[indicatorHighSignal], this.Data, indicatorHigh, quant.Series[TickMonitor[indicatorHighSignal]].QDatum.Value));
				for (i = 0; i <= 0; i++) { quantTracker[i] = quantTracker[i + 1]; }
				quantTracker[i] = quant.Series[TickMonitor[indicatorHighSignal]].QDatum.Value;
				TickMonitor[indicatorLowSignal] = TickMonitor[indicatorHighSignal] = -1;
			}
		}

		private bool isBackTrackLow(Indicator quant, int lookback, out int tickBack)
		{
			int i; tickBack = 0;
			if (lookback >= Ticks) { return false; }
			for (i = 0; i < lookback; i++)
			{
				if (quant.Series[Ticks].QDatum.Value <= quant.Series[Ticks - i - 1].QDatum.Value) { return false; }
			}
			//the edge case error here is that in some cases, quant[Ticks - i] == quant[Ticks - i - 1], which causes the function to return a lowest point "false" value
			if ((Ticks - i - 1) == 0 && quant.Series[Ticks - i].QDatum.Value > quant.Series[Ticks - i - 1].QDatum.Value) { tickBack = i; return true; }
			int x = i;
			while (quant.Series[Ticks - i] == quant.Series[Ticks - x - 1]) { x++; if (Ticks - x - 1 < 0) { x--; break; } }
			return (quant.Series[Ticks - i].QDatum.Value < quant.Series[Ticks - x - 1].QDatum.Value);
		}

		private bool isBackTrackHigh(Indicator quant, int lookback)
		{
			int i;
			if (lookback >= Ticks) { return false; }
			for (i = 0; i < lookback; i++)
			{
				if (quant.Series[Ticks - i].QDatum.Value >= quant.Series[Ticks - i - 1].QDatum.Value) { return false; }
			}
			if ((Ticks - i - 1) == 0 && quant.Series[Ticks - i].QDatum.Value < quant.Series[Ticks - i - 1].QDatum.Value) { return true; }
			int x = i;
			while (quant.Series[Ticks - i] == quant.Series[Ticks - x - 1]) { x++; if (Ticks - x - 1 < 0) { x--; break; } }
			return (quant.Series[Ticks - i].QDatum.Value > quant.Series[Ticks - x - 1].QDatum.Value);
		}

		private bool isBackTrackTrendUp(Indicator quant, int lookback)
		{
			int count = lookback;
			for (int i = 1; i <= count; i++)
			{
				lookback -= 1;
				if (!(quant.Series[Ticks - lookback].QDatum.Value > quant.Series[Ticks - lookback - 1].QDatum.Value)) { return false; }
			}
			return true;
		}

		private bool isBackTrackTrendDown(Indicator quant, int lookback, double wobble = 0D)
		{
			int count = lookback;
			for (int i = 1; i <= count; i++)
			{
				lookback -= 1;
				if (!(quant.Series[Ticks - lookback].QDatum.Value - wobble < quant.Series[Ticks - lookback - 1].QDatum.Value)) { return false; }
			}
			return true;
		}

		private bool isQuantCompareLT(Indicator quantLow, Indicator quantHigh, int lookback = 0)
		{
			return (quantLow.Series[Ticks - lookback].QDatum.Value < quantHigh.Series[Ticks - lookback].QDatum.Value);
		}

		private bool isQuantCompareGT(Indicator quantHigh, Indicator quantLow, int lookback = 0)
		{
			return (quantHigh.Series[Ticks - lookback].QDatum.Value > quantLow.Series[Ticks - lookback].QDatum.Value);
		}

		private bool isIndicatorLowTickCompareGT(SignalType indicatorLow, SignalType indicatorHigh)
		{
			return (TickMonitor[indicatorLow] > TickMonitor[indicatorHigh]);
		}

		private bool isIndicatorHighTickCompareGT(SignalType indicatorLow, SignalType indicatorHigh)
		{
			return (TickMonitor[indicatorHigh] > TickMonitor[indicatorLow]);
		}

		private bool isQuantSlopeExceedThreshold(Indicator quant, int lookback, Operator op = Operator.LT, double threshold = -0.01D)
		{
			switch (op)
			{
				case Operator.LT:
					return (Ticks > lookback && quantSlope(quant, lookback) < threshold);

				case Operator.GT:
					return (Ticks > lookback && quantSlope(quant, lookback) > threshold);

				case Operator.LE:
					return (Ticks > lookback && quantSlope(quant, lookback) <= threshold);

				default:
					return (Ticks > lookback && quantSlope(quant, lookback) >= threshold);
			}
		}

		private double quantSlope(Indicator quant, int lookback)
		{
			return (lookback > Ticks) ? 0 : ((quant.Series[Ticks].QDatum.Value - quant.Series[Ticks - lookback].QDatum.Value) / lookback);
		}

		private bool deltaExceedsThreshold(SignalType indicatorLow, SignalType indicatorHigh, int threshold)
		{
			return (TickMonitor[indicatorHigh] - TickMonitor[indicatorLow] > threshold);
		}

		private bool deltaExceedsThreshold(Indicator quantUpper, Indicator quantLower, int threshold, int lookback = 0)
		{
			return (quantUpper.Series[Ticks - lookback].QDatum.Value - quantLower.Series[Ticks - lookback].QDatum.Value > threshold);
		}

		private bool deltaExceedsThreshold(Indicator quantUpper, Indicator quantLower, double threshold, int lookback = 0)
		{
			return (quantUpper.Series[Ticks - lookback].QDatum.Value - quantLower.Series[Ticks - lookback].QDatum.Value > threshold);
		}

		private bool deltaExceedsThreshold(Indicator quant, SignalType indicator, double threshold)
		{
			return (quant.Series[Ticks].QDatum.Value - quant.Series[TickMonitor[indicator]].QDatum.Value > threshold);
		}

		private bool deltaBelowThreshold(Indicator quantUpper, Indicator quantLower, double threshold, int lookback = 0)
		{
			return (quantUpper.Series[Ticks - lookback].QDatum.Value - quantLower.Series[Ticks - lookback].QDatum.Value <= threshold);
		}

		private bool isTickDeltaAboveThreshold(SignalType indicator, int threshold)
		{
			return (Ticks - TickMonitor[indicator] >= threshold);
		}

		private bool isTickDeltaBelowThreshold(SignalType indicator, int threshold)
		{
			return (Ticks - TickMonitor[indicator] <= threshold);
		}

		private bool isValueAboveThreshold(Indicator quant, double threshold)
		{
			return (quant.Series[Ticks].QDatum.Value > threshold);
		}

		private bool isValueBelowThreshold(Indicator quant, double threshold)
		{
			return (quant.Series[Ticks].QDatum.Value < threshold);
		}

		private bool isTickDeltaAtMark(SignalType indicator, int mark)
		{
			return (Ticks - TickMonitor[indicator] == mark);
		}

		#endregion Tools/Utilities
	}
}