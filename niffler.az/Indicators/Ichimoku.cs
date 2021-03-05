using Niffler.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Niffler.Indicators
{
	public class Ichimoku : VIndicator<IchimokuSeries>
	{
		protected int Fast = 9;
		protected int Med = 26;
		protected int Slow = 26;

		public Ichimoku()
		{
		}

		public Ichimoku(int fast, int med, int slow)
		{
			this.Fast = fast;
			this.Med = med;
			this.Slow = slow;
		}

		public override IchimokuSeries Calculate()
		{
			IchimokuSeries ichimokuSerie = new IchimokuSeries();

			List<double> highList = Quotes.Select(x => x.High).ToList();
			List<double> lowList = Quotes.Select(x => x.Low).ToList();

			// TurningLine
			List<double?> runMaxFast = Custom.RunMax(highList, Fast);
			List<double?> runMinFast = Custom.RunMin(lowList, Fast);
			List<double?> runMaxMed = Custom.RunMax(highList, Med);
			List<double?> runMinMed = Custom.RunMin(lowList, Med);
			List<double?> runMaxSlow = Custom.RunMax(highList, Slow);
			List<double?> runMinSlow = Custom.RunMin(lowList, Slow);

			for (int i = 0; i < Quotes.Count(); i++)
			{
				if (i >= Fast - 1)
				{
					ichimokuSerie.ConversionLine.Add((runMaxFast[i] + runMinFast[i]) / 2);
				}
				else
				{
					ichimokuSerie.ConversionLine.Add(null);
				}

				if (i >= Med - 1)
				{
					ichimokuSerie.BaseLine.Add((runMaxMed[i] + runMinMed[i]) / 2);
					ichimokuSerie.LeadingSpanA.Add((ichimokuSerie.BaseLine[i] + ichimokuSerie.ConversionLine[i]) / 2);
				}
				else
				{
					ichimokuSerie.BaseLine.Add(null);
					ichimokuSerie.LeadingSpanA.Add(null);
				}

				if (i >= Slow - 1)
				{
					ichimokuSerie.LeadingSpanB.Add((runMaxSlow[i] + runMinSlow[i]) / 2);
				}
				else
				{
					ichimokuSerie.LeadingSpanB.Add(null);
				}
			}

			// shift to left Med
			List<double?> laggingSpan = new List<double?>();//Tuples.Select(x => x.QuantValue).ToList();//new double?[Tuples.Count()];
			for (int i = 0; i < Tuples.Count(); i++)
			{
				laggingSpan.Add(null);
			}
			for (int i = 0; i < Tuples.Count(); i++)
			{
				if (i >= Med - 1)
				{
					laggingSpan[i - (Med - 1)] = Tuples[i].QDatum;
				}
			}
			ichimokuSerie.LaggingSpan = laggingSpan;

			return ichimokuSerie;
		}
	}
}