using Niffler.Data;
using Niffler.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Niffler.Indicators
{
	/// <summary>
	/// Indicator Reference for +/-DI: https://www.fmlabs.com/reference/default.htm?url=DI.htm
	/// </summary>
	public class ADX : VIndicator<ADXSeries>
	{
		public ADX()
		{
		}

		public ADX(int period)
		{
			this.Period = period;
		}

		public override ADXSeries Calculate()
		{
			ADXSeries adxSeries = new ADXSeries();
			adxSeries.Time = Quotes.Select(x => x.Time).ToList();
			Momentum momentum = new Momentum(false);
			momentum.LoadSeries(Quotes, VIndicator<Indicator>.QuoteField.High);
			Indicator highMomentums = momentum.Calculate();

			momentum = new Momentum(false);
			momentum.LoadSeries(Quotes, VIndicator<Indicator>.QuoteField.Low);
			Indicator lowMomentums = momentum.Calculate();

			//Is this inversion of quant values necessary???
			for (int i = 0; i < lowMomentums.Series.Count; i++)
			{
				if (lowMomentums.Series[i].QDatum.HasValue)
				{
					lowMomentums.Series[i].QDatum *= -1;
				}
			}

			//DMIp <- ifelse( dH==dL | (dH< 0 & dL< 0), 0, ifelse( dH >dL, dH, 0 ) )
			List<IndicatorTuple> DMIPositives = new List<IndicatorTuple>() { new IndicatorTuple(Quotes.First().Time, 0D) };
			// DMIn <- ifelse( dH==dL | (dH< 0 & dL< 0), 0, ifelse( dH <dL, dL, 0 ) )
			List<IndicatorTuple> DMINegatives = new List<IndicatorTuple>() { new IndicatorTuple(Quotes.First().Time, 0D) };
			for (int i = 1; i < Quotes.Count(); i++)
			{
				if (highMomentums.Series[i].QDatum == lowMomentums.Series[i].QDatum || (highMomentums.Series[i].QDatum < 0 & lowMomentums.Series[i].QDatum < 0))
				{
					DMIPositives.Add(new IndicatorTuple(Quotes[i].Time, 0D));
				}
				else
				{
					if (highMomentums.Series[i].QDatum > lowMomentums.Series[i].QDatum)
					{
						DMIPositives.Add(new IndicatorTuple(Quotes[i].Time, highMomentums.Series[i].QDatum));
					}
					else
					{
						DMIPositives.Add(new IndicatorTuple(Quotes[i].Time, 0D));
					}
				}

				if (highMomentums.Series[i].QDatum == lowMomentums.Series[i].QDatum || (highMomentums.Series[i].QDatum < 0 & lowMomentums.Series[i].QDatum < 0))
				{
					DMINegatives.Add(new IndicatorTuple(Quotes[i].Time, 0D));
				}
				else
				{
					if (highMomentums.Series[i].QDatum < lowMomentums.Series[i].QDatum)
					{
						DMINegatives.Add(new IndicatorTuple(Quotes[i].Time, lowMomentums.Series[i].QDatum));
					}
					else
					{
						DMINegatives.Add(new IndicatorTuple(Quotes[i].Time, 0D));
					}
				}
			}

			ATR atr = new ATR();
			atr.LoadSeries(Quotes);
			adxSeries.TrueRange = atr.Calculate().TrueRange;

			List<IndicatorTuple> trSum = wilderSum(adxSeries.TrueRange);

			// DIp <- 100 * wilderSum(DMIp, n=n) / TRsum
			List<IndicatorTuple> DIPositives = new List<IndicatorTuple>();
			List<IndicatorTuple> wilderSumOfDMIp = wilderSum(DMIPositives);
			for (int i = 0; i < wilderSumOfDMIp.Count; i++)
			{
				if (wilderSumOfDMIp[i].QDatum.HasValue)
				{
					DIPositives.Add(new IndicatorTuple(Quotes[i].Time, (wilderSumOfDMIp[i].QDatum.Value / trSum[i].QDatum.Value) * 100));
				}
				else
				{
					DIPositives.Add(new IndicatorTuple(Quotes[i].Time, 0D));
				}
			}
			adxSeries.DIPositive = DIPositives;

			// DIn <- 100 * wilderSum(DMIn, n=n) / TRsum
			List<IndicatorTuple> DINegatives = new List<IndicatorTuple>();
			List<IndicatorTuple> wilderSumOfDMIn = wilderSum(DMINegatives);
			for (int i = 0; i < wilderSumOfDMIn.Count; i++)
			{
				if (wilderSumOfDMIn[i].QDatum.HasValue)
				{
					DINegatives.Add(new IndicatorTuple(Quotes[i].Time, (wilderSumOfDMIn[i].QDatum.Value / trSum[i].QDatum.Value) * 100));
				}
				else
				{
					DINegatives.Add(new IndicatorTuple(Quotes[i].Time, 0D));
				}
			}
			adxSeries.DINegative = DINegatives;

			// DX  <- 100 * ( abs(DIp - DIn) / (DIp + DIn) )
			List<IndicatorTuple> DX = new List<IndicatorTuple>();
			for (int i = 0; i < Quotes.Count(); i++)
			{
				if (DIPositives[i].QDatum.HasValue)
				{
					double? dx = 100 * (Math.Abs(DIPositives[i].QDatum.Value - DINegatives[i].QDatum.Value) / (DIPositives[i].QDatum.Value + DINegatives[i].QDatum.Value));
					DX.Add(new IndicatorTuple(Quotes[i].Time, dx));
				}
				else
				{
					DX.Add(new IndicatorTuple(Quotes[i].Time, 0D));
				}
			}
			adxSeries.DX = DX;

			for (int i = 0; i < Tuples.Count(); i++)
			{
				Tuples[i].QDatum = DX[i].QDatum ?? 0D;
			}

			EMA ema = new EMA(Period, true);
			ema.LoadSeries(Tuples.Skip(Period).ToList());
			Indicator emaSeries = ema.Calculate();
			for (int i = Period - 1; i >= 0; i--)
			{
				emaSeries.Series.Insert(0, new IndicatorTuple(Tuples[i].Time, 0D));
			}
			adxSeries.ADX = emaSeries.Series;

			return adxSeries;
		}

		private List<IndicatorTuple> wilderSum(List<IndicatorTuple> values)
		{
			IndicatorTuple[] wilderSums = new IndicatorTuple[values.Count];

			int beg = Period - 1;
			double sum = 0;
			int i;
			for (i = 0; i < beg; i++)
			{
				/* Account for leading NAs in input */
				if (!values[i].QDatum.HasValue)
				{
					wilderSums[i] = new IndicatorTuple(values[i].Time, null);
					beg++;
					wilderSums[beg] = new IndicatorTuple(values[i].Time, 0D);
					continue;
				}
				/* Set leading NAs in output */
				if (i < beg)
				{
					wilderSums[i] = new IndicatorTuple(values[i].Time, null);
				}
				/* Calculate raw sum to start */
				sum += values[i].QDatum.Value;
			}
			//sum = values.Take(beg).Sum().Value;
			wilderSums[beg] = new IndicatorTuple(values[i].Time, (values[i].QDatum + ((sum * (Period - 1)) / Period)));

			/* Loop over non-NA input values */
			for (i = beg + 1; i < values.Count; i++)
			{
				wilderSums[i] = new IndicatorTuple(values[i].Time, (values[i].QDatum + ((wilderSums[i - 1].QDatum * (Period - 1)) / Period)));
			}

			return wilderSums.ToList();
		}
	}
}