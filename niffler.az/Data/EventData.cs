using Niffler.Indicators;
using Niffler.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Niffler.Data
{
	/// <summary>
	/// Essential for every trading module.
	/// EventData is responsible for loading each type of indicator you want to use in your algorithm
	/// with quote data.  You decide which indicators you want to use and which period levels to use.
	/// </summary>
	public class EventData
	{
		public EventData()
		{
		}

		public EventData(List<Quote> quotes, string symbol, DateTime tradeDateStart, double tradePrincipal, TradeDispatchDTO dto)
		{
			this.Symbol = symbol;
			this.TradePrincipal = tradePrincipal;
			this.TradeDateStart = tradeDateStart;
			this.IntraDayQuotes = quotes;
			this.TradeDispatch = dto;
		}

		/// <summary>
		/// Load each type of indicator you want to monitor with quote data.
		/// Quote data is currently configured to pull at least the last 100 minutes of 1-minute quotes,
		/// which will include previous session at start of current session, NOT pre-market quote data.
		/// If you want pre-market data, you will need to work on getting that from the provider somehow.
		/// Note that some indicators use volume data, which would be sparse in pre-market quotes compared to regular session.
		/// This demo code currently only uses today-only quote data for some volume consuming indicators, such as the OBV.
		/// </summary>
		public void LoadSourceData()
		{
			var todayOnlyQuotes = (from quote in IntraDayQuotes where quote.Time >= this.TradeDateStart select new Quote(quote)).ToList();
			Trix7 = buildIndicator(IntraDayQuotes, new TRIX(7));
			Trix10 = buildIndicator(IntraDayQuotes, new TRIX(10));
			Trix15 = buildIndicator(IntraDayQuotes, new TRIX(15));
			Trix25 = buildIndicator(IntraDayQuotes, new TRIX(25));
			var adx = new ADX(20); adx.LoadSeries(IntraDayQuotes); var series = adx.Calculate();
			PlusDI20 = new Indicator(series.DIPositive.ToArray());
			MinusDI20 = new Indicator(series.DINegative.ToArray());
			this.T3 = buildIndicator(IntraDayQuotes, new T3(2));
			this.EFI4 = buildIndicator(IntraDayQuotes, new EFI(5));
			this.SVI4 = buildIndicator(IntraDayQuotes, new SVI(4));
			this.OBV3 = buildIndicator(todayOnlyQuotes, new OBV());
			//WMA wma = new WMA(4); wma.LoadSeries(this.PlusDI20.Tuples, false);
			//this.PDI20WMA = wma.Calculate();
			//wma.LoadSeries(this.MinusDI20.Tuples, false);
			//this.MDI20WMA = wma.Calculate();
			T3 t3 = new T3(2, .4, 2);
			t3.LoadSeries(this.PlusDI20.Tuples, false);
			this.PDI20T2 = t3.Calculate();
			t3.LoadSeries(this.MinusDI20.Tuples, false);
			this.MDI20T2 = t3.Calculate();
			t3 = new T3(2, .5, 3);
			t3.LoadSeries(this.PlusDI20.Tuples, false);
			this.PDI20T3 = t3.Calculate();
			t3.LoadSeries(this.MinusDI20.Tuples, false);
			this.MDI20T3 = t3.Calculate();
			t3 = new T3(3, .4, 3);
			t3.LoadSeries(this.OBV3.Tuples, false);
			this.OBV3 = t3.Calculate();
		}

		private Indicator buildIndicator(List<Quote> quotes, VIndicator<Indicator> indicator)
		{
			indicator.LoadSeries(quotes);
			return indicator.Calculate();
		}

		/// <summary>
		/// Populates custom indicators and removes time-series data prior to current session.
		/// </summary>
		/// <param name="tickPeriod">Period to use for custom indicators.</param>
		/// <param name="slopePeriod">Slope period to use for custom slope indicators.</param>
		/// <param name="slopeLookback">Number of ticks to look back for custom slope indicators.</param>
		public void FillQuants(int tickPeriod, int slopePeriod, int slopeLookback)
		{
			int ticks = 0, backstopPeriod = (tickPeriod * 2), smoothingFactor = 2;
			double ema1 = 0D;
			//After all indicator calculations have been run, remove time-series data
			//for everything prior to start of session.
			IntraDayQuotes.RemoveAll(x => x.Time < this.TradeDateStart);
			Trix7.Series.RemoveAll(x => x.Time < this.TradeDateStart);
			Trix10.Series.RemoveAll(x => x.Time < this.TradeDateStart);
			Trix15.Series.RemoveAll(x => x.Time < this.TradeDateStart);
			Trix25.Series.RemoveAll(x => x.Time < this.TradeDateStart);
			PlusDI20.Series.RemoveAll(x => x.Time < this.TradeDateStart);
			MinusDI20.Series.RemoveAll(x => x.Time < this.TradeDateStart);
			PDI20T2.Series.RemoveAll(x => x.Time < this.TradeDateStart);
			MDI20T2.Series.RemoveAll(x => x.Time < this.TradeDateStart);
			PDI20T3.Series.RemoveAll(x => x.Time < this.TradeDateStart);
			MDI20T3.Series.RemoveAll(x => x.Time < this.TradeDateStart);
			EFI4.Series.RemoveAll(x => x.Time < this.TradeDateStart);
			SVI4.Series.RemoveAll(x => x.Time < this.TradeDateStart);
			T3.Series.RemoveAll(x => x.Time < this.TradeDateStart);
			//START: KEEP
			this.Ticks = IntraDayQuotes.Count;
			this.CurrentVolume = this.IntraDayQuotes.Sum(x => x.Volume);
			this.LastQuoteVolume = this.IntraDayQuotes.Last().Volume;
			this.LastQuotePrice = this.IntraDayQuotes.Last().Close;
			this.CurrentOBV3 = (long)this.OBV3.Tuples.Last().QDatum.Value;
			//END: KEEP
			//Initialize arrays for custom indicator calculations
			double[] PDI20TemaQV = new double[Ticks], MDI20TemaQV = new double[Ticks];
			double[] PDI20SlopeQV = new double[Ticks], MDI20SlopeQV = new double[Ticks];
			double[] PDI20SMAQV = new double[Ticks], MDI20SMAQV = new double[Ticks];
			double[] PlusDI20QV = new double[Ticks], MinusDI20QV = new double[Ticks];
			//Populate custom indicator arrays
			for (ticks = 0; ticks < Ticks; ticks++)
			{
				PlusDI20QV[ticks] = PlusDI20.Tuples[ticks].QDatum.Value;
				MinusDI20QV[ticks] = MinusDI20.Tuples[ticks].QDatum.Value;
				if (slopeLookback < 10 && IntraDayQuotes[ticks].Time.TimeOfDay > Cache.TimeBoundary[Boundary.T1030]) { slopeLookback *= 2; }
				if (ticks >= backstopPeriod)
				{
					MDI20TemaQV[ticks] = Custom.TEMA(MinusDI20QV, ticks, tickPeriod, smoothingFactor);
					PDI20TemaQV[ticks] = Custom.TEMA(PlusDI20QV, ticks, tickPeriod, smoothingFactor);
					MDI20SlopeQV[ticks] = Custom.Slope(MDI20TemaQV, ticks, slopePeriod);
					PDI20SlopeQV[ticks] = Custom.Slope(PDI20TemaQV, ticks, slopePeriod);
					MDI20SMAQV[ticks] = (slopeLookback <= ticks) ? Custom.SMA(MDI20SlopeQV, ticks, slopeLookback) : 0;
					PDI20SMAQV[ticks] = (slopeLookback <= ticks) ? Custom.SMA(PDI20SlopeQV, ticks, slopeLookback) : 0;
					ema1 = Custom.EMA(MDI20SlopeQV, MDI20SMAQV[ticks], ticks, tickPeriod, smoothingFactor);
					double MDI20TemaSlopeQV = (slopeLookback < ticks) ? Custom.TEMA(MDI20SlopeQV, ema1, ticks, slopeLookback, smoothingFactor) : 0;
					ema1 = Custom.EMA(PDI20SlopeQV, PDI20SMAQV[ticks], ticks, tickPeriod, smoothingFactor);
					double PDI20TemaSlopeQV = (slopeLookback < ticks) ? Custom.TEMA(PDI20SlopeQV, ema1, ticks, slopeLookback, smoothingFactor) : 0;
					MDI20TEMASlope.Add(new IndicatorTuple(IntraDayQuotes[ticks].Time, MDI20TemaSlopeQV));
					PDI20TEMASlope.Add(new IndicatorTuple(IntraDayQuotes[ticks].Time, PDI20TemaSlopeQV));
				}
				else
				{
					MDI20TEMASlope.Add(new IndicatorTuple(IntraDayQuotes[ticks].Time, 0));
					PDI20TEMASlope.Add(new IndicatorTuple(IntraDayQuotes[ticks].Time, 0));
				}
			}
			//KEEP
			this.Ticks = ticks;
		}

		#region Enums

		/// <summary>
		/// You can change the enums for each type of indicator configuration you want to monitor
		/// </summary>
		public enum QuantType
		{
			Trix7,
			Trix10,
			Trix15,
			Trix25,
			PlusDI20,
			MinusDI20,
			IntraDayQuotes
		};

		#endregion Enums

		#region EventPropertiesIndicators

		public Dictionary<QuantType, string> QuantSource { get; set; } = new Dictionary<QuantType, string>();
		public string Symbol { get; set; }
		public long CurrentVolume { get; set; }
		public long CurrentOBV3 { get; set; }
		public long LastQuoteVolume { get; set; }
		public double LastQuotePrice { get; set; }
		public DateTime TradeDateStart { get; set; }
		public TradeDispatchDTO TradeDispatch { get; private set; }
		public double TradePrincipal { get; set; }
		public List<Quote> IntraDayQuotes { get; set; } = new List<Quote>();
		public Indicator T3 { get; set; } = new Indicator();
		public Indicator Trix7 { get; set; } = new Indicator();
		public Indicator Trix10 { get; set; } = new Indicator();
		public Indicator Trix15 { get; set; } = new Indicator();
		private Indicator Trix25Src { get; set; }
		public Indicator Trix25 { get; set; } = new Indicator();
		public Indicator PlusDI20 { get; set; } = new Indicator();
		public Indicator MinusDI20 { get; set; } = new Indicator();

		//public Indicator PDI20TSlope { get; set; }
		//public Indicator MDI20TSlope { get; set; }
		public Indicator PDI20T2 { get; set; }

		public Indicator MDI20T2 { get; set; }
		public Indicator PDI20T3 { get; set; }
		public Indicator MDI20T3 { get; set; }
		public Indicator PDI20TEMASlope { get; set; } = new Indicator();
		public Indicator MDI20TEMASlope { get; set; } = new Indicator();
		public Indicator EFI4 { get; set; } = new Indicator(); //Triple Smoothed SMA(EFI) @period=4
		public Indicator SVI4 { get; set; } = new Indicator(); //Smoothed Volume Indicator
		public Indicator OBV3 { get; set; } = new Indicator(); //On Balance Volume Indicator (OBV)
		public Indicator ADL4 { get; set; } = new Indicator(); //Accumulation/Distribution Indicator (AD)
		public double[] MDI20SlopePeakTracker { get; set; } = new double[2] { -10, -10 };
		public int Ticks { get; set; } = -1;

		#endregion EventPropertiesIndicators
	}
}