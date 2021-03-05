using System.Collections.Generic;

namespace Niffler.Data
{
	/// <summary>
	/// Encapsulates dictionary of event status by status type.
	/// </summary>
	public class EventState
	{
		public EventState()
		{
			Status = new Dictionary<StatusType, bool>()
			{
				{ StatusType.IsDIPositive, false },
				{ StatusType.IsDIInverted, false },
				{ StatusType.IsDIT2Positive, false },
				{ StatusType.IsDIT2Inverted, false },
				{ StatusType.IsPDI20T2XOverMDI20T2, false },
				{ StatusType.IsPDI20T3XOverMDI20T3, false },
				{ StatusType.IsInPosition, false },
				{ StatusType.IsMaxBuysBefore1030Reached, false },
				{ StatusType.IsBefore0945TBndry, true },
				{ StatusType.IsAfter1000TBndry, false },
				{ StatusType.IsBefore1000TBndry, true },
				{ StatusType.IsBefore1030TBndry, true },
				{ StatusType.IsAfter1030TBndry, false },
				{ StatusType.IsBefore1400TBndry, true },
				{ StatusType.IsBefore1500TBndry, true },
				{ StatusType.IsBefore1530TBndry, true },
				{ StatusType.IsWithinMiddayBlackout, false },
				{ StatusType.IsAfter1550TBndry, false },
				{ StatusType.IsBtwn1000And1100, false },
				{ StatusType.IsBtwn1030And1100, false },
				{ StatusType.IsBtwn1400And1500, false },
				{ StatusType.IsBtwn1500And1550, false },
				{ StatusType.Trix10Under15, false },
				{ StatusType.Trix10Under25, false },
				{ StatusType.Trix15Under25, false },
				{ StatusType.Trix7Under10, false },
				{ StatusType.Trix7Under15, false },
				{ StatusType.Trix7Under25, false },
				{ StatusType.Trix7Over25, false },
				{ StatusType.Trix25Over7, false },
				{ StatusType.Trix7Over10, false },
				{ StatusType.Trix7Over15, false },
				{ StatusType.Trix10Over15, false },
				{ StatusType.Trix10Over25, false },
				{ StatusType.Trix25_7OverDelta, false },
				{ StatusType.Trix7UptrendLB1, false },
				{ StatusType.Trix7DowntrendLB2, false },
				{ StatusType.Trix10DowntrendLB2, false },
				{ StatusType.Trix10Under25AtPeakTrix7, false },
				{ StatusType.Trix7UptrendLB2, false },
				{ StatusType.Trix7Over25AtPeakTrix7, false },
				{ StatusType.Trix7Under25AtLowTrix10, false },
				{ StatusType.Trix10Under25AtLowTrix10, false },
				{ StatusType.Trix10Under15AtLowTrix10, false },
				{ StatusType.Trix7Under15AtLowTrix10, false },
				{ StatusType.Trix10UptrendLB2, false },
				{ StatusType.Trix15UptrendLB2, false },
				{ StatusType.Trix25DowntrendLB60, false },
				{ StatusType.Trix10Under15SinceClose_GR4, false },
				{ StatusType.Trix10DowntrendLB30_GR5, false },
				{ StatusType.Trix15DowntrendLB20_GR5, false },
				{ StatusType.Trix7XUnder15DurationGT30_GR6, false },
				{ StatusType.Trix10XUnder25DurationGT30_GR6, false },
				{ StatusType.Trix25UptrendDurationGE30_GR7, false },
				{ StatusType.IsDeltaPDI20_MDI20gt10, false },
				{ StatusType.IsDeltaPDI20_MDI20gt5, false },
				{ StatusType.IsDeltaPDI20_MDI20gt2, false },
				{ StatusType.IsDeltaMDI20_PDI20gt5, false },
				{ StatusType.IsDeltaPDI20T2_MDI20T2le5, false },
				{ StatusType.IsDeltaPDI20T2_MDI20T2le9, false },
				{ StatusType.IsDeltaPDI20_MDI20le5, false },
				{ StatusType.IsDeltaPDI20_MDI20le1, false },
				{ StatusType.IsDeltaMDI20_PDI20le5, false },
				{ StatusType.IsDIInvertedAtTrix10Low, false },
				{ StatusType.IsDIPositiveAtTrix10Low, false },
				{ StatusType.IsEFI4AfterPeak, false },
				{ StatusType.IsEFI4AfterLow, false },
				{ StatusType.IsEFI4BelowBuyLine, false },
				{ StatusType.IsEFI4AboveSellLine, false },
				{ StatusType.IsSVI4AfterPeak, false },
				{ StatusType.IsSVI4AfterLow, false },
				{ StatusType.IsSVI4BelowBuyLine, false },
				{ StatusType.IsSVI4AboveSellLine, false },
				{ StatusType.IsT3AfterPeak, false },
				{ StatusType.IsT3AfterLow, false },
				{ StatusType.IsPDI20T2AfterPeak, false },
				{ StatusType.IsPDI20T2AfterLow, false },
				{ StatusType.IsMDI20T2AfterPeak, false },
				{ StatusType.IsMDI20T2AfterLow, false },
				{ StatusType.IsPDI20T3AfterPeak, false },
				{ StatusType.IsPDI20T3AfterLow, false },
				{ StatusType.IsMDI20T3AfterPeak, false },
				{ StatusType.IsMDI20T3AfterLow, false },
				{ StatusType.IsTrix7xDTrix15, false },
				{ StatusType.IsTrix7x2DTrix15, false },
				{ StatusType.IsTrix7AfterPeak, false },
				{ StatusType.IsTrix7AfterLow, false },
				{ StatusType.IsTrix10AfterPeak, false },
				{ StatusType.IsTrix10AfterPeakGTTrix15, false },
				{ StatusType.IsTrix10AfterLow, false },
				{ StatusType.IsTrix15AfterPeak, false },
				{ StatusType.IsTrix15AfterLow, false },
				{ StatusType.IsTrix15W0005AfterLow, false },
				{ StatusType.IsTrix15W0005AfterPeak, false },
				{ StatusType.IsTrix25W0005AfterLow, false },
				{ StatusType.IsTrix25W0005AfterPeak, false },
				{ StatusType.IsTrix25W0008AfterLow, false },
				{ StatusType.IsTrix25W0008AfterPeak, false },
				{ StatusType.IsTrix25AfterPeak, false },
				{ StatusType.IsTrix25AfterLow, false },
				{ StatusType.IsTicksAfterTrix10LowLE5, false },
				{ StatusType.IsTicksAfterTrix10LowGE1, false },
				{ StatusType.IsTicksAfterTrix7LowLE5, false },
				{ StatusType.IsTicksAfterTrix7LowGE1, false },
				{ StatusType.Is1TickAfterTrix7Peak, false },
				{ StatusType.Is2TicksAfterTrix7Peak, false },
				{ StatusType.IsTicksAfterTrix7PeakGE1, false },
				{ StatusType.IsTicksAfterTrix7PeakGE2, false },
				{ StatusType.Is2TicksAfterTrix15Low, false },
				{ StatusType.Is1TickAfterTrix10Low, false },
				{ StatusType.Is2TicksAfterTrix10Low, false },
				{ StatusType.IsDIInvertedDelta7DurationGE2, false },
				{ StatusType.IsDIPositiveDelta7DurationGE2, false },
				{ StatusType.IsDIPositiveDelta2DurationGE2, false },
				{ StatusType.IsDIPositiveDelta5DurationGT0, false },
				{ StatusType.IsDIPositiveDurationGT10, false },
				{ StatusType.IsDIPositiveDurationGTLimit, false },
				{ StatusType.IsDIInvertedDurationGT30_GR11, false },
				{ StatusType.IsPDI20gt30, false },
				{ StatusType.IsPDI20SteepEnough, false },
				{ StatusType.IsTrix10SlopeSteepEnough, false },
				{ StatusType.IsTrix7SlopeSteepEnough, false },
				{ StatusType.IsTrix7SteepDecline, false },
				{ StatusType.IsTrix7RiseAboveThreshold, false },
				{ StatusType.IsWithinT3BuyRange, true },
				{ StatusType.IsMDI20TEMASlopeAfterLow, false },
				{ StatusType.IsMDI20TEMASlopeAfterHigh, false },
				{ StatusType.IsPDI20TEMASlopeAfterLow, false },
				{ StatusType.IsPDI20TEMASlopeAfterHigh, false },
				{ StatusType.IsPDI20SlopeAboveMDI20Slope, false },
				{ StatusType.IsPDI20SlopeBelowMDI20Slope, false },
				{ StatusType.IsPDI20SlopeUp, false },
				{ StatusType.IsMDI20AboveThreshold, false },
				{ StatusType.IsPDI20AboveThreshold, false },
				{ StatusType.IsPDI20BelowThreshold, false },

				{ StatusType.IsMDI20MeanDeltaBelowT1P1, false },
				{ StatusType.IsMDI20MeanDeltaAboveT2P1, false },
				{ StatusType.IsVolumeDeltaAboveT1P1, false },
				{ StatusType.IsPriceDeltaBelowT1P1, false },
				{ StatusType.IsPDIMeanFactorAboveT1P1, false },
				{ StatusType.IsMDIMeanFactorAboveT1P1, false },
				{ StatusType.IsDIDeltaBelowT1P2, false },
				{ StatusType.IsDIDeltaAboveT2P2, false },
				{ StatusType.IsMDIPDIMeanDeltaAboveT1P2, false },
				{ StatusType.IsMDIPDIMeanDeltaBelowT2P2, false },
				{ StatusType.IsPDI20T3AboveT1P2, false },
				{ StatusType.IsPDI20T3BelowT2P2, false },
				{ StatusType.IsTrix7DeltaBelowT1P3, false },
				{ StatusType.IsTrix7DeltaAboveT2P3, false },
				{ StatusType.IsPDI20T3AboveT1P3, false },
				{ StatusType.IsEFIDeltaBelowT1P3, false },
				{ StatusType.IsSVIWithinT1P3, false },
				{ StatusType.IsDIDeltaBelowT1P3, false },
				{ StatusType.IsDIDeltaInvAboveT1P3, false },
				{ StatusType.IsDIDeltaInvBelowT2P3, false },
				{ StatusType.IsTrix7DeltaWithinT1P3, false },
				{ StatusType.IsTrix7DeltaWithinT2P3, false },
				{ StatusType.IsVolumeDeltaBelowT1P3, false },
				{ StatusType.IsPriceDeltaBelowT1P3, false },
				{ StatusType.IsMDI20MeanDeltaWithinT1P3, false },

				{ StatusType.IsPDI20SlopeBelowThreshold, false },
				{ StatusType.IsDIPositiveAboveThreshold, false },
				{ StatusType.IsDeltaPDI20_MDI20AboveThreshold, false },
				{ StatusType.IsDeltaMDI20_PDI20AboveThreshold, false },
				{ StatusType.IsSellPressureDeclining, false },
				{ StatusType.IsAboveStopLossThreshold, false },
				{ StatusType.IsPriceAboveBackstop1Pct, false },
				{ StatusType.IsPriceAboveBackstop8Pct, false },
				{ StatusType.IsBackstop1PctActivated, false },
				{ StatusType.IsBackstop8PctActivated, false }
			};
		}

		public Dictionary<StatusType, bool> Status { get; set; }

		//LB=LookBack;PDI20=Plus Directional Indicator @interval 20;MDI20=Minus Directional Indicator @interval 20;
		//Btwn=Between;TBndry=Time Boundary;GT=Greater Than;LT=Less Than;GE=Greater Than or Equal To; LE=Less Than or Equal To
		public enum StatusType
		{
			IsDIPositive, //Is the PDI20 > MDI20
			IsDIInverted, //Is the MDI20 > PDI20
			IsDIT2Positive, //Is the PDI20 > MDI20
			IsDIT2Inverted, //Is the MDI20 > PDI20
			IsPDI20T2XOverMDI20T2,
			IsPDI20T3XOverMDI20T3,
			IsInPosition, //In current buy position
			IsMaxBuysBefore1030Reached,
			IsBefore0945TBndry,
			IsAfter1000TBndry, //Event State after 10:30 time boundary
			IsBefore1000TBndry, //Event State before 10:00 time boundary
			IsBefore1030TBndry, //Event State before 10:30 time boundary
			IsAfter1030TBndry, //Event State after 10:30 time boundary
			IsBefore1400TBndry, //Event State before 14:00 time boundary
			IsAfter1550TBndry, //Event State after 15:50 time boundary
			IsBtwn1000And1100, //Event State between 10:00 and 11:00 time boundaries
			IsBtwn1030And1100, //Event State between 10:30 and 11:00 time boundaries
			IsBtwn1400And1500, //Event State between 14:00 and 15:00 time boundaries
			IsBtwn1500And1550, //Event State between 15:00 and 15:50 time boundaries
			IsBefore1500TBndry, //Event State before 15:00 time boundary
			IsBefore1530TBndry, //Event State before 15:30 time boundary
			IsWithinMiddayBlackout, //Event state between 12:40 and 13:30 EST
			Trix10Under15, //Is Trix10 < Trix15
			Trix10Under25, //Is Trix10 < Trix25
			Trix15Under25, //Is Trix10 < Trix25
			Trix7Under15, //Is Trix7 < Trix15
			Trix7Under10, //Is Trix7 < Trix10
			Trix7Under25, //Is Trix7 < Trix25
			Trix7Over25, //Is Trix7 > Trix25
			Trix25Over7, //Is Trix25 > Trix7
			Trix7Over10, //Is Trix7 > Trix10
			Trix7Over15, //Is Trix7 > Trix15
			Trix10Over15, //Is Trix10 > Trix15
			Trix10Over25, //Is Trix10 > Trix25
			Trix25_7OverDelta, //Is Trix25-Trix7 > TrixDelta; no LB
			Trix7UptrendLB1, //Is Trix7 trending up for last 1 ticks
			Trix7DowntrendLB2, //Is Trix7 trending down for last 2 ticks
			Trix10DowntrendLB2, //Is Trix10 trending down for last 2 ticks
			Trix10Under25AtPeakTrix7, //Is Trix10 < Trix25 @ peak of Trix7
			Trix7UptrendLB2, //Is Trix7 trending up for last 2 ticks
			Trix7Over25AtPeakTrix7, //Is Trix7 > Trix25 @ peak of Trix7
			Trix7Under25AtLowTrix10, //TRIX7<TRIX25 @ TRIX10 low & following 2 ticks
			Trix10Under25AtLowTrix10, //TRIX10<TRIX25 @ TRIX10 low & following 2 ticks
			Trix10Under15AtLowTrix10,
			Trix7Under15AtLowTrix10,
			Trix10UptrendLB2, //Is Trix10 trending up for last 2 ticks
			Trix15UptrendLB2, //Is Trix15 trending up for last 2 ticks
			Trix25DowntrendLB60, //Is Trix25 trending down for last 60+ ticks
			Trix10Under15SinceClose_GR4, //Track TRIX10 from low; reset if TRIX10 peaks only when above TRIX15; if not since last buy/sell, don't buy again until TRIX10 passes TRIX15 again.
			Trix10DowntrendLB30_GR5,
			Trix15DowntrendLB20_GR5, //Track TRIX15 (<10:30): if in decline 20+min w/ <= .0005 wobble: no buy until TRIX15 up > .0005;
			Trix10XUnder25DurationGT30_GR6, //Track TRIX25 (>10:30):from point TRIX10 X below TRIX25: if TRIX25 in decline 30+ min w/ <= .0005 wobble: no buy until (TRIX25 up > .0005 & IsTrix10AfterLow) || (TRIX7/TRIX10/TRIX15 Uptrend LB2);
			Trix7XUnder15DurationGT30_GR6, //Track TRIX25 (>10:30):from point TRIX10 X below TRIX25: if TRIX25 in decline 30+ min w/ <= .0005 wobble: no buy until (TRIX25 up > .0005 & IsTrix10AfterLow) || (TRIX7/TRIX10/TRIX15 Uptrend LB2);
			Trix25UptrendDurationGE30_GR7, //Track TRIX25 (>10:30): if TRIX25 in incline 30+ min w/ <= .0008 wobble: no buy until (TRIX25 up > .0008 & IsTrix10AfterLow);
			IsDeltaPDI20_MDI20gt10, //Delta PDI20-MDI20 > 10
			IsDeltaPDI20_MDI20gt5, //Delta PDI20-MDI20 > 5
			IsDeltaPDI20_MDI20gt2, //Delta PDI20-MDI20 > 2
			IsDeltaMDI20_PDI20gt5, //Delta MDI20-PDI20 > 5
			IsDeltaPDI20T2_MDI20T2le9, //Is PDI20T2-MDI20T2 <= 9; no LB
			IsDeltaPDI20T2_MDI20T2le5, //Is PDI20T2-MDI20T2 <= 5; no LB
			IsDeltaPDI20_MDI20le5, //Is PDI20-MDI20 <= 5; no LB
			IsDeltaPDI20_MDI20le1, //Is PDI20-MDI20 <= 1; no LB
			IsDeltaMDI20_PDI20le5, //Is MDI20-PDI20 <= 5; no LB
			IsDIInvertedAtTrix10Low, //Is the MDI20 > PDI20 @ Trix10 Low
			IsDIPositiveAtTrix10Low, //Is the MDI20 > PDI20 @ Trix10 Low
			IsEFI4AfterPeak,
			IsEFI4AfterLow,
			IsEFI4BelowBuyLine,
			IsEFI4AboveSellLine,
			IsT3AfterPeak,
			IsT3AfterLow,
			IsSVI4AfterPeak,
			IsSVI4AfterLow,
			IsSVI4BelowBuyLine,
			IsSVI4AboveSellLine,
			IsPDI20T2AfterPeak,
			IsPDI20T2AfterLow,
			IsMDI20T2AfterPeak,
			IsMDI20T2AfterLow,
			IsPDI20T3AfterPeak,
			IsPDI20T3AfterLow,
			IsMDI20T3AfterPeak,
			IsMDI20T3AfterLow,
			IsTrix7xDTrix15,
			IsTrix7x2DTrix15,
			IsTrix7AfterPeak,
			IsTrix7AfterLow,
			IsTrix10AfterPeak,
			IsTrix10AfterPeakGTTrix15,
			IsTrix10AfterLow,
			IsTrix15AfterPeak,
			IsTrix15AfterLow,
			IsTrix15W0005AfterLow,
			IsTrix15W0005AfterPeak,
			IsTrix25W0005AfterLow,
			IsTrix25W0005AfterPeak,
			IsTrix25W0008AfterLow,
			IsTrix25W0008AfterPeak,
			IsTrix25AfterPeak,
			IsTrix25AfterLow,
			IsTicksAfterTrix10LowLE5,
			IsTicksAfterTrix10LowGE1,
			IsTicksAfterTrix7LowLE5,
			IsTicksAfterTrix7LowGE1,
			Is1TickAfterTrix7Peak, //Is Trix7 @peak +1 tick
			Is2TicksAfterTrix7Peak,
			IsTicksAfterTrix7PeakGE1,
			IsTicksAfterTrix7PeakGE2,
			Is2TicksAfterTrix15Low,
			Is1TickAfterTrix10Low,
			Is2TicksAfterTrix10Low,
			IsDIInvertedDelta7DurationGE2,
			IsDIPositiveDelta7DurationGE2,
			IsDIPositiveDelta2DurationGE2,
			IsDIPositiveDelta5DurationGT0,
			IsDIPositiveDurationGT10,
			IsDIPositiveDurationGTLimit,
			IsDIInvertedDurationGT30_GR11, //From last point MDI20 X Over PDI20, if duration DI Inverted > 30 ticks; don't buy until (+DI20) > (-DI20) || TRIX15 trends up 2 ticks from TRIX15 low.
			IsPDI20gt30,
			IsPDI20SteepEnough,
			IsTrix10SlopeSteepEnough,
			IsTrix7SlopeSteepEnough,
			IsTrix7SteepDecline,
			IsTrix7RiseAboveThreshold,
			IsWithinT3BuyRange,
			IsMDI20TEMASlopeAfterLow,
			IsMDI20TEMASlopeAfterHigh,
			IsPDI20TEMASlopeAfterLow,
			IsPDI20TEMASlopeAfterHigh,
			IsPDI20SlopeAboveMDI20Slope,
			IsPDI20SlopeBelowMDI20Slope,
			IsDeltaMDI20_PDI20AboveThreshold,
			IsDeltaPDI20_MDI20AboveThreshold,
			IsPDI20SlopeUp,
			IsMDI20AboveThreshold,
			IsPDI20AboveThreshold,
			IsPDI20BelowThreshold,

			IsMDI20MeanDeltaBelowT1P1, //P1=9:30-9:45; T1=threshold <= -3
			IsMDI20MeanDeltaAboveT2P1, //P1=9:30-9:45; T2=threshold >= 8
			IsVolumeDeltaAboveT1P1, //P1=9:30-9:45; (VolumeEnd-VolumeStart) > -16000
			IsPriceDeltaBelowT1P1, //P1=9:30-9:45; (PriceEnd-PriceStart) < 3.05
			IsPDIMeanFactorAboveT1P1, //P1=9:30-9:45; T1=threshold > 3
			IsMDIMeanFactorAboveT1P1, //P1=9:30-9:45; T1=threshold > 1

			IsDIDeltaBelowT1P2, //P1=9:45-10:30; T1=threshold DIDelta < -17.5
			IsDIDeltaAboveT2P2,  //P1=9:45-10:30; T1=threshold DIDelta > -8.5
			IsMDIPDIMeanDeltaAboveT1P2,  //P1=9:45-10:30; T1=threshold (MDIMeanDelta - PDIMeanDelta) > -1
			IsMDIPDIMeanDeltaBelowT2P2, //P1=9:45-10:30; T1=threshold (MDIMeanDelta - PDIMeanDelta) < -4.5
			IsPDI20T3AboveT1P2, //P1=9:45-10:30; T1=threshold PDI20T3End >= 19
			IsPDI20T3BelowT2P2, //P1=9:45-10:30; T1=threshold PDI20T3End < 14

			IsTrix7DeltaBelowT1P3, //P1=10:30-15:50; T1=threshold  < .0307D
			IsTrix7DeltaAboveT2P3, //P1 = 10:30-15:50; T2 = threshold > .078D
			IsPDI20T3AboveT1P3, //P1=10:30-15:50; T1=threshold PDI20T3End > 17.25
			IsEFIDeltaBelowT1P3, //P1=10:30-15:50; T1=threshold EFIEnd < -1550
			IsSVIWithinT1P3, //P1=10:30-15:50; T2=threshold SVIEnd between -327 and -80
			IsDIDeltaBelowT1P3, //P1=10:30-15:50; T1=threshold DIDelta <= .75
			IsDIDeltaInvAboveT1P3, //P1=10:30-15:50; T1=threshold DIDeltaInv > 4
			IsDIDeltaInvBelowT2P3, //P1=10:30-15:50; T1=threshold DIDeltaInv < 3.5
			IsTrix7DeltaWithinT1P3, //P1=10:30-15:50; T1=threshold not(TrixDelta between .0825 and .1045)
			IsTrix7DeltaWithinT2P3, //P1=10:30-15:50; T1=threshold not(TrixDelta between .0087 and .0137)
			IsVolumeDeltaBelowT1P3, //P1=10:30-15:50; (VolumeEnd-VolumeStart) < 15900
			IsPriceDeltaBelowT1P3, //P1=10:30-15:50; (PriceEnd-PriceStart) <= .86
			IsMDI20MeanDeltaWithinT1P3, //P1=10:30-15:50; T1=threshold MDIMeanDelta between -.21 and .21

			IsPDI20SlopeBelowThreshold,
			IsDIPositiveAboveThreshold,
			IsSellPressureDeclining,
			IsAboveStopLossThreshold,
			IsPriceAboveBackstop1Pct,
			IsPriceAboveBackstop8Pct,
			IsBackstop1PctActivated,
			IsBackstop8PctActivated
		};
	}
}