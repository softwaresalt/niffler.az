using Niffler.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Niffler.Indicators
{
	public static class Custom
	{
		public static double StandardDeviation(List<double> valueList)
		{
			double M = 0.0;
			double S = 0.0;
			int k = 1;
			foreach (double value in valueList)
			{
				double tmpM = M;
				M += (value - tmpM) / k;
				S += (value - tmpM) * (value - M);
				k++;
			}
			return Math.Sqrt(S / (k - 2));
		}

		public static List<double?> RunMax(List<double> list, int period)
		{
			List<double?> maxList = new List<double?>();

			for (int i = 0; i < list.Count; i++)
			{
				if (i >= period - 1)
				{
					double max = 0.0;
					for (int j = i - (period - 1); j <= i; j++)
					{
						if (list[j] > max)
						{
							max = list[j];
						}
					}

					maxList.Add(max);
				}
				else
				{
					maxList.Add(null);
				}
			}

			return maxList;
		}

		public static List<double?> RunMin(List<double> list, int period)
		{
			List<double?> minList = new List<double?>();

			for (int i = 0; i < list.Count; i++)
			{
				if (i >= period - 1)
				{
					double min = Double.MaxValue;
					for (int j = i - (period - 1); j <= i; j++)
					{
						if (list[j] < min)
						{
							min = list[j];
						}
					}

					minList.Add(min);
				}
				else
				{
					minList.Add(null);
				}
			}

			return minList;
		}

		public static double SMA(double[] quant, int startTick, int tickPeriod)
		{
			double total = 0D;
			for (int i = startTick - tickPeriod; i <= startTick; i++) { total += quant[i]; }
			return (total / tickPeriod);
		}

		public static Indicator SMA(IndicatorTuple[] quant, int startTick, int tickPeriod)
		{
			Indicator indicator = new Indicator();
			//int limit = tickPeriod * 2;
			tickPeriod *= 2;
			for (int i = 0; i < quant.Count(); i++)
			{
				//if (tickPeriod < limit && quant[i].Time.TimeOfDay > Cache.TimeBoundary[Boundary.T1030]) { tickPeriod = limit; }
				double total = (i >= startTick) ? quant.Skip(i - tickPeriod + 1).Take(tickPeriod).Sum(x => x.QDatum.Value) / tickPeriod : 0D;
				indicator.Add(new IndicatorTuple(quant[i].Time, total));
			}
			return indicator;
		}

		public static double EMA(double[] quant, int currentTick, int tickPeriod, int smoothingFactor = 2)
		{
			double ema = SMA(quant, currentTick - tickPeriod, tickPeriod);
			return EMA(quant, ema, currentTick, tickPeriod, smoothingFactor);
		}

		public static double EMA(double[] quant, double ema, int currentTick, int tickPeriod, int smoothingFactor = 2)
		{
			double smoothingConstant = smoothingFactor / (1 + tickPeriod);
			for (int i = currentTick - tickPeriod; i <= currentTick; i++)
			{
				ema = ((quant[i] - ema) * smoothingConstant) + ema;
			}
			return ema;
		}

		public static double EMA(IndicatorTuple[] quant, double ema, int currentTick, int tickPeriod, int smoothingFactor = 2)
		{
			double smoothingConstant = smoothingFactor / (1 + tickPeriod);
			for (int i = currentTick - tickPeriod; i <= currentTick; i++)
			{
				ema = ((quant[i].QDatum.Value - ema) * smoothingConstant) + ema;
			}
			return ema;
		}

		public static double TEMA(double[] quant, int currentTick, int tickPeriod, int smoothingFactor = 2)
		{
			double ema1 = EMA(quant, currentTick, tickPeriod, smoothingFactor);
			return TEMA(quant, ema1, currentTick, tickPeriod, smoothingFactor);
		}

		public static double TEMA(double[] quant, double ema1, int currentTick, int tickPeriod, int smoothingFactor = 2)
		{
			double ema2 = EMA(quant, ema1, currentTick, tickPeriod, smoothingFactor);
			double ema3 = EMA(quant, ema2, currentTick, tickPeriod, smoothingFactor);
			return Math.Round((3 * ema1) - (3 * ema2) + ema3, 4);
		}

		public static double TEMA(IndicatorTuple[] quant, double ema1, int currentTick, int tickPeriod)
		{
			double ema2 = EMA(quant, ema1, currentTick, tickPeriod);
			double ema3 = EMA(quant, ema2, currentTick, tickPeriod);
			return Math.Round((3 * ema1) - (3 * ema2) + ema3, 4);
		}

		public static Indicator TEMA(Indicator quant, int tickPeriod)
		{
			Indicator indicator = new Indicator();
			Indicator sma = SMA(quant.Tuples, 0, tickPeriod);
			for (int i = 0; i < quant.Series.Count; i++)
			{
				double tema = 0D;
				if (i >= tickPeriod)
				{
					double ema = EMA(quant.Tuples, sma.Tuples[i].QDatum.Value, i, tickPeriod);
					tema = Custom.TEMA(quant.Tuples, ema, i, tickPeriod);
				}
				indicator.Add(new IndicatorTuple(quant.Tuples[i].Time, tema));
			}
			return indicator;
		}

		public static double TRIX(double[] quant, int currentTick, int tickPeriod, int smoothingFactor = 2)
		{
			double currentTEMA = TEMA(quant, currentTick, tickPeriod, smoothingFactor);
			double priorTEMA = TEMA(quant, currentTick - 1, tickPeriod, smoothingFactor);
			return Math.Round((currentTEMA - priorTEMA) / priorTEMA, 4);
		}

		public static Indicator Slope(Indicator quant, int currentTick = 0, int lookback = 6)
		{
			Indicator indicator = new Indicator();
			foreach (IndicatorTuple tuple in quant.Series)
			{
				if (lookback < 10 && tuple.Time.TimeOfDay > Cache.TimeBoundary[Boundary.T1030]) { lookback *= 2; }
				indicator.Add(new IndicatorTuple(tuple.Time,
					(lookback > currentTick) ? 0D : ((tuple.QDatum.Value - quant.Tuples[currentTick - lookback].QDatum.Value) / lookback)
					));
				currentTick++;
			}
			return indicator;
		}

		public static double Slope(double[] quant, int currentTick, int lookback)
		{
			return (lookback > currentTick) ? 0 : ((quant[currentTick] - quant[currentTick - lookback]) / lookback);
		}

		/// <summary>
		/// Sloped TEMA
		/// </summary>
		/// <returns></returns>
		public static Indicator STEMA(Indicator sma, Indicator slope, int slopePeriod, int slopeLookback)
		{
			Indicator indicator = new Indicator();
			for (int i = 0; i < slope.Series.Count; i++)
			{
				double tema = 0D;
				if (i >= slopeLookback && i >= slopePeriod)
				{
					double ema = Custom.EMA(slope.Tuples, sma.Tuples[i].QDatum.Value, i, slopePeriod);
					tema = Custom.TEMA(slope.Tuples, ema, i, slopeLookback);
				}
				indicator.Add(new IndicatorTuple(slope.Tuples[i].Time, tema));
			}
			return indicator;
		}
	}
}