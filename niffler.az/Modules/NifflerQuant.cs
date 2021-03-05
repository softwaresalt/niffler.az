using Niffler.Data;
using System;
using static Niffler.Data.EventState;
using c = Niffler.Modules.APCA.Common;

namespace Niffler.Modules
{
	/// <summary>
	/// Encapsulates quantitative logic for NifflerMod
	/// Signal logic depends on EventMonitor and EventData for signals
	/// </summary>
	public class NifflerQuant
	{
		public NifflerQuant()
		{
		}

		public NifflerQuant(EventData quantData)
		{
			this.Symbol = quantData.Symbol;
			this.TradePrincipal = quantData.TradePrincipal;
			quantData.LoadSourceData();
			quantData.FillQuants(4, 6, 5);
			QMonitor = new EventMonitor(quantData, new EventState());
			this.ruleGroup = quantData.TradeDispatch.RuleGroup;
		}

		public string Symbol { get; set; }
		public double TradePrincipal { get; set; }
		public EventMonitor QMonitor { get; set; }

		//private int ledgerSign = 1;
		private TradeRuleGroup ruleGroup = TradeRuleGroup.None;

		private BuySellCase ruleCase;

		public enum Action
		{
			None,
			Group1Buy,
			Group1Sell,
			Group2Buy,
			Group2Sell,
			Group3Buy,
			Group3Sell
		};

		public Trade ComputeTrades()
		{
			Trade trade = null; int i;
			for (i = 0; i < QMonitor.Data.Ticks; i++)
			{
				if (QMonitor.CurrentState.Status[StatusType.IsBefore1030TBndry] && i < 2) { continue; }
				QMonitor.EvaluateState(i);
			};
			i--;
			if (i < 4) { return trade; }
			setCurrentState(QMonitor.Data.TradeDispatch);
			//ledgerSign = (QMonitor.CurrentState.Status[StatusType.IsInPosition]) ? 1 : -1;
			if (QMonitor.Data.IntraDayQuotes[i].Time.TimeOfDay < Cache.TimeBoundary[Boundary.T1000] && QMonitor.Data.TradeDispatch.TradesBeforeTimeBoundary == 1)
			{ QMonitor.DurationMonitor[DType.MaxTradesBefore1030TimeBoundary] = 2; }
			switch (assignAction())
			{
				case Action.Group1Buy:
					ruleGroup = TradeRuleGroup.Group1;
					trade = c.RegisterBuyPosition(QMonitor.Data, ruleGroup, ruleCase, i);
					QMonitor.CurrentState.Status[StatusType.IsInPosition] = true;
					QMonitor.Data.TradeDispatch.TradesBeforeTimeBoundary++;
					break;

				case Action.Group2Buy:
					ruleGroup = TradeRuleGroup.Group2;
					trade = c.RegisterBuyPosition(QMonitor.Data, ruleGroup, ruleCase, i);
					QMonitor.CurrentState.Status[StatusType.IsInPosition] = true;
					break;

				case Action.Group1Sell:
				case Action.Group2Sell:
					trade = c.RegisterSellPosition(QMonitor.Data, QMonitor.Ticks, sellCase: ruleCase);
					QMonitor.CurrentState.Status[StatusType.IsInPosition] = false;
					QMonitor.CurrentState.Status[StatusType.IsPriceAboveBackstop1Pct] = false;
					QMonitor.CurrentState.Status[StatusType.IsBackstop1PctActivated] = false;
					break;

				case Action.None:
					if (QMonitor.CurrentState.Status[StatusType.IsAfter1550TBndry]) { trade = new Trade() { State = TradeState.MarketEoD }; };
					break;
			}
			return trade;
		}

		private void setCurrentState(TradeDispatchDTO dto)
		{
			QMonitor.CurrentState.Status[StatusType.IsInPosition] = dto.InPosition;
			if (dto.LastBuyMinute > TimeSpan.Zero)
			{
				QMonitor.TickMonitor[SignalType.LastBuyTick] = QMonitor.Data.IntraDayQuotes.FindIndex(x => x.Time.TimeOfDay == dto.LastBuyMinute);
			}
			if (dto.LastSellMinute > TimeSpan.Zero)
			{
				QMonitor.TickMonitor[SignalType.LastSellTick] = QMonitor.Data.IntraDayQuotes.FindIndex(x => x.Time.TimeOfDay == dto.LastSellMinute);
			}
		}

		private Action assignAction()
		{
			if (ruleGroup == TradeRuleGroup.None && QMonitor.CurrentState.Status[StatusType.IsBefore1030TBndry] && !QMonitor.CurrentState.Status[StatusType.IsInPosition])
			{
				if (isRuleGroup1BuyCase1) { ruleCase = BuySellCase.RGBuyCase1; return Action.Group1Buy; }
			}

			if (ruleGroup == TradeRuleGroup.Group1 && QMonitor.CurrentState.Status[StatusType.IsInPosition])
			{
				if (isRuleGroup1SellCase1) { ruleCase = BuySellCase.RGSellCase1; return Action.Group1Sell; }
				if (isRuleGroup1SellCase2) { ruleCase = BuySellCase.RGSellCase2; return Action.Group1Sell; }
				if (isRuleGroup1SellCase3) { ruleCase = BuySellCase.RGSellCase3; return Action.Group1Sell; }
				if (isRuleGroupCommonSellCaseBackstop) { ruleCase = BuySellCase.CGSellCaseBackstop; return Action.Group1Sell; }
				if (isRuleGroupCommonSellCaseStopLoss) { ruleCase = BuySellCase.CGSellCaseStopLoss; return Action.Group1Sell; }
			}

			if (ruleGroup == TradeRuleGroup.None && QMonitor.CurrentState.Status[StatusType.IsAfter1030TBndry] && !QMonitor.CurrentState.Status[StatusType.IsInPosition])
			{
				if (isRuleGroup2BuyCase1) { ruleCase = BuySellCase.RGBuyCase1; return Action.Group2Buy; }
			}

			if (ruleGroup == TradeRuleGroup.Group2 && QMonitor.CurrentState.Status[StatusType.IsInPosition])
			{
				if (isRuleGroup2SellCase1) { ruleCase = BuySellCase.RGSellCase1; return Action.Group2Sell; }
				if (isRuleGroup2SellCase2) { ruleCase = BuySellCase.RGSellCase2; return Action.Group2Sell; }
				if (isRuleGroupCommonSellCaseStopLoss) { ruleCase = BuySellCase.CGSellCaseStopLoss; return Action.Group2Sell; }
				if (isRuleGroupCommonSellCaseEOD) { ruleCase = BuySellCase.CGSellCaseEOD; return Action.Group2Sell; }
			}

			return Action.None;
		}

		#region RuleGroup1Buy

		private bool isRuleGroup1BuyCase1
		{
			get
			{
				int lag = (int)QMonitor.Data.IntraDayQuotes[QMonitor.Ticks].Time.Subtract(QMonitor.Data.IntraDayQuotes[0].Time).TotalMinutes;
				return (
						QMonitor.CurrentState.Status[StatusType.IsBefore1030TBndry] && lag > 5
						&& (QMonitor.Data.TradeDispatch.TradesBeforeTimeBoundary < QMonitor.DurationMonitor[DType.MaxTradesBefore1030TimeBoundary])
						&& QMonitor.CurrentState.Status[StatusType.IsTicksAfterTrix7LowGE1] //Less profitable, but much higher Avg P/L rate.
						&& QMonitor.CurrentState.Status[StatusType.IsWithinT3BuyRange]
						&& QMonitor.CurrentState.Status[StatusType.IsPDI20T3AfterLow]
						&& (
							(QMonitor.CurrentState.Status[StatusType.IsBefore0945TBndry] &&
							(
								QMonitor.CurrentState.Status[StatusType.IsVolumeDeltaAboveT1P1]
								|| QMonitor.CurrentState.Status[StatusType.IsMDIMeanFactorAboveT1P1]
								|| QMonitor.CurrentState.Status[StatusType.IsMDI20MeanDeltaAboveT2P1]
							)
							&& QMonitor.CurrentState.Status[StatusType.IsPriceDeltaBelowT1P1]
						)
							||
							(!QMonitor.CurrentState.Status[StatusType.IsBefore0945TBndry] && QMonitor.CurrentState.Status[StatusType.IsTrix7SlopeSteepEnough]
								&& !QMonitor.CurrentState.Status[StatusType.IsSVI4AfterPeak]
								&& (QMonitor.CurrentState.Status[StatusType.IsMDIPDIMeanDeltaAboveT1P2] || QMonitor.CurrentState.Status[StatusType.IsMDIPDIMeanDeltaBelowT2P2])
								&& (QMonitor.CurrentState.Status[StatusType.IsPDI20T3AboveT1P2] || QMonitor.CurrentState.Status[StatusType.IsPDI20T3BelowT2P2])
							)
						)
						&& !QMonitor.CurrentState.Status[StatusType.IsMaxBuysBefore1030Reached]
						&& !QMonitor.CurrentState.Status[StatusType.IsInPosition]
				);
			}
		}

		#endregion RuleGroup1Buy

		#region RuleGroup1Sell

		private bool isRuleGroup1SellCase1
		{
			get
			{
				return (QMonitor.CurrentState.Status[StatusType.IsInPosition]
					&& QMonitor.CurrentState.Status[StatusType.Trix7Over10]
					&& ((QMonitor.CurrentState.Status[StatusType.IsTrix7AfterPeak] && QMonitor.CurrentState.Status[StatusType.IsBefore1000TBndry])
					|| (QMonitor.CurrentState.Status[StatusType.IsTrix10AfterPeak] && QMonitor.CurrentState.Status[StatusType.IsAfter1000TBndry])
					)
					&& (QMonitor.CurrentState.Status[StatusType.IsEFI4AfterPeak] || (QMonitor.CurrentState.Status[StatusType.IsT3AfterPeak] && QMonitor.CurrentState.Status[StatusType.IsBefore1000TBndry]))
					&& QMonitor.CurrentState.Status[StatusType.IsPDI20T2AfterPeak]
					&& QMonitor.TickMonitor[SignalType.MDI20T3Low] > QMonitor.TickMonitor[SignalType.LastBuyTick]
					&& QMonitor.TickMonitor[SignalType.PDI20T3High] > QMonitor.TickMonitor[SignalType.LastBuyTick]
				);
			}
		}

		private bool isRuleGroup1SellCase2
		{
			get
			{
				return (QMonitor.CurrentState.Status[StatusType.IsInPosition]
					&& QMonitor.CurrentState.Status[StatusType.Trix7Over15]
					&& QMonitor.CurrentState.Status[StatusType.IsDeltaPDI20T2_MDI20T2le9]
					&& QMonitor.TickMonitor[SignalType.MDI20T3Low] > QMonitor.TickMonitor[SignalType.LastBuyTick]
					&& QMonitor.TickMonitor[SignalType.PDI20T3High] > QMonitor.TickMonitor[SignalType.LastBuyTick]
				);
			}
		}

		private bool isRuleGroup1SellCase3
		{
			get
			{
				return (QMonitor.CurrentState.Status[StatusType.IsInPosition]
					&& QMonitor.CurrentState.Status[StatusType.IsDIT2Positive]
					&& QMonitor.CurrentState.Status[StatusType.Trix7Over15]
					&& QMonitor.CurrentState.Status[StatusType.IsPDI20T3AfterPeak]
					&& QMonitor.CurrentState.Status[StatusType.IsBefore0945TBndry]
					&& QMonitor.TickMonitor[SignalType.MDI20T3Low] > QMonitor.TickMonitor[SignalType.LastBuyTick]
					&& QMonitor.TickMonitor[SignalType.PDI20T3High] > QMonitor.TickMonitor[SignalType.LastBuyTick]
				 );
			}
		}

		#endregion RuleGroup1Sell

		#region RuleGroup2Buy

		private bool isRuleGroup2BuyCase1
		{
			get
			{
				return (QMonitor.CurrentState.Status[StatusType.IsAfter1030TBndry]
					&& !QMonitor.CurrentState.Status[StatusType.IsWithinMiddayBlackout]
					&& QMonitor.CurrentState.Status[StatusType.IsBefore1530TBndry]
					&& QMonitor.CurrentState.Status[StatusType.IsWithinT3BuyRange]
					&& QMonitor.CurrentState.Status[StatusType.IsDIInvertedAtTrix10Low]
					&& QMonitor.CurrentState.Status[StatusType.IsPDI20T3AfterLow]
					&& (
						(QMonitor.CurrentState.Status[StatusType.IsTicksAfterTrix7LowGE1] && QMonitor.CurrentState.Status[StatusType.IsTicksAfterTrix10LowLE5])
						|| QMonitor.CurrentState.Status[StatusType.IsEFI4AfterLow]
					)
					&& QMonitor.CurrentState.Status[StatusType.IsTrix7SlopeSteepEnough]
					&& (QMonitor.CurrentState.Status[StatusType.IsEFI4BelowBuyLine] && !QMonitor.CurrentState.Status[StatusType.IsSVI4AboveSellLine])
					&& (
						QMonitor.CurrentState.Status[StatusType.IsPDI20T3AboveT1P3] || QMonitor.CurrentState.Status[StatusType.IsEFIDeltaBelowT1P3] || QMonitor.CurrentState.Status[StatusType.IsSVIWithinT1P3]
					)
					&& QMonitor.CurrentState.Status[StatusType.IsDIDeltaBelowT1P3]
					&& (QMonitor.CurrentState.Status[StatusType.IsDIDeltaInvAboveT1P3] || QMonitor.CurrentState.Status[StatusType.IsDIDeltaInvBelowT2P3])
					&& QMonitor.CurrentState.Status[StatusType.IsVolumeDeltaBelowT1P3]
					&& QMonitor.CurrentState.Status[StatusType.IsPriceDeltaBelowT1P3]
					&& !QMonitor.CurrentState.Status[StatusType.IsTrix7DeltaWithinT1P3]
					&& !QMonitor.CurrentState.Status[StatusType.IsTrix7DeltaWithinT2P3]
					&& !QMonitor.CurrentState.Status[StatusType.IsMDI20MeanDeltaWithinT1P3]
					&& !QMonitor.CurrentState.Status[StatusType.IsInPosition]
					&& (QMonitor.CurrentState.Status[StatusType.IsTrix7DeltaBelowT1P3] || QMonitor.CurrentState.Status[StatusType.IsTrix7DeltaAboveT2P3])
					&& !(QMonitor.CurrentState.Status[StatusType.Trix10DowntrendLB30_GR5] || QMonitor.CurrentState.Status[StatusType.Trix15DowntrendLB20_GR5] || QMonitor.CurrentState.Status[StatusType.Trix25DowntrendLB60])
					&& !QMonitor.CurrentState.Status[StatusType.IsDIInvertedDurationGT30_GR11]
				);
			}
		}

		#endregion RuleGroup2Buy

		#region RuleGroup2Sell

		private bool isRuleGroup2SellCase1
		{
			get
			{
				return (QMonitor.CurrentState.Status[StatusType.IsAfter1030TBndry]
					&& QMonitor.CurrentState.Status[StatusType.IsInPosition]
					&& QMonitor.CurrentState.Status[StatusType.IsDIPositive]
					&& QMonitor.CurrentState.Status[StatusType.IsTicksAfterTrix7PeakGE1]
					&& QMonitor.CurrentState.Status[StatusType.IsPDI20T3AfterPeak]
					&& QMonitor.CurrentState.Status[StatusType.IsDeltaPDI20T2_MDI20T2le9]
				&& (
					(QMonitor.CurrentState.Status[StatusType.Trix7Under15] && QMonitor.CurrentState.Status[StatusType.Trix10Under15])
				|| (QMonitor.CurrentState.Status[StatusType.IsTrix7SteepDecline] && QMonitor.CurrentState.Status[StatusType.IsPDI20SlopeBelowThreshold])
				)
				);
			}
		}

		private bool isRuleGroup2SellCase2
		{
			get
			{
				return (QMonitor.CurrentState.Status[StatusType.IsAfter1030TBndry]
					&& QMonitor.CurrentState.Status[StatusType.IsInPosition]
					&& QMonitor.CurrentState.Status[StatusType.IsDIPositive]
					&& QMonitor.CurrentState.Status[StatusType.Trix7Under25]
					&& (QMonitor.CurrentState.Status[StatusType.IsDeltaPDI20T2_MDI20T2le9] || QMonitor.CurrentState.Status[StatusType.IsTrix7SteepDecline])
					&& (QMonitor.CurrentState.Status[StatusType.IsTrix7AfterPeak] || QMonitor.CurrentState.Status[StatusType.IsEFI4AfterPeak] || QMonitor.CurrentState.Status[StatusType.IsT3AfterPeak])
					&& QMonitor.CurrentState.Status[StatusType.IsPDI20SlopeBelowThreshold]
					&& QMonitor.CurrentState.Status[StatusType.IsSellPressureDeclining]
				);
			}
		}

		#endregion RuleGroup2Sell

		#region CommonSellGroup

		private bool isRuleGroupCommonSellCaseEOD
		{
			get
			{
				return (QMonitor.CurrentState.Status[StatusType.IsAfter1550TBndry]
					&& QMonitor.CurrentState.Status[StatusType.IsInPosition]
				);
			}
		}

		private bool isRuleGroupCommonSellCaseBackstop
		{
			get
			{
				return (QMonitor.CurrentState.Status[StatusType.IsInPosition]
					&& (
						(QMonitor.CurrentState.Status[StatusType.IsBackstop1PctActivated] && !QMonitor.CurrentState.Status[StatusType.IsPriceAboveBackstop1Pct])
					|| (QMonitor.CurrentState.Status[StatusType.IsBackstop8PctActivated] && !QMonitor.CurrentState.Status[StatusType.IsPriceAboveBackstop8Pct])
					)
					&& !QMonitor.CurrentState.Status[StatusType.IsPDI20T3AfterLow]
				&& QMonitor.CurrentState.Status[StatusType.IsDeltaPDI20T2_MDI20T2le9]
				);
			}
		}

		private bool isRuleGroupCommonSellCaseStopLoss
		{
			get
			{
				return (QMonitor.CurrentState.Status[StatusType.IsInPosition]
					&& (QMonitor.CurrentState.Status[StatusType.IsAboveStopLossThreshold])
				);
			}
		}

		#endregion CommonSellGroup
	}
}