using Niffler.Interfaces;
using System.Linq;

namespace Niffler.Indicators
{
	/// <summary>
	/// Aroon
	/// </summary>
	public class Aroon : VIndicator<AroonSeries>
	{
		public Aroon(int period)
		{
			Period = period;
		}

		/// <summary>
		/// Aroon up: {((number of periods) - (number of periods since highest high)) / (number of periods)} x 100
		/// Aroon down: {((number of periods) - (number of periods since lowest low)) / (number of periods)} x 100
		/// </summary>
		/// <see cref="http://www.investopedia.com/ask/answers/112814/what-aroon-indicator-formula-and-how-indicator-calculated.asp"/>
		/// <returns></returns>
		public override AroonSeries Calculate()
		{
			AroonSeries aroonSerie = new AroonSeries();
			for (int i = 0; i < Quotes.Count(); i++)
			{
				if (i >= Period)
				{
					double aroonUp = CalculateAroonUp(i);
					double aroonDown = CalculateAroonDown(i);

					aroonSerie.Down.Add(aroonDown);
					aroonSerie.Up.Add(aroonUp);
				}
				else
				{
					aroonSerie.Down.Add(null);
					aroonSerie.Up.Add(null);
				}
			}

			return aroonSerie;
		}

		private double CalculateAroonUp(int i)
		{
			var maxIndex = FindMax(i - Period, i);
			var up = CalcAroon(i - maxIndex);
			return up;
		}

		private double CalculateAroonDown(int i)
		{
			var minIndex = FindMin(i - Period, i);
			var down = CalcAroon(i - minIndex);
			return down;
		}

		private double CalcAroon(int dayPeriod)
		{
			var result = ((Period - dayPeriod)) * ((double)100 / Period);
			return result;
		}

		private int FindMin(int startIndex, int endIndex)
		{
			var min = double.MaxValue;
			var index = startIndex;
			for (var i = startIndex; i <= endIndex; i++)
			{
				if (min < Quotes[i].Low)
					continue;

				min = Quotes[i].Low;
				index = i;
			}
			return index;
		}

		private int FindMax(int startIndex, int endIndex)
		{
			var max = double.MinValue;
			var index = startIndex;
			for (var i = startIndex; i <= endIndex; i++)
			{
				if (max > Quotes[i].High)
					continue;

				max = Quotes[i].High;
				index = i;
			}
			return index;
		}
	}
}