using System.Collections.Generic;
using System.Linq;

namespace Niffler.Data
{
	/// <summary>
	/// Basic type used by all indicators for storing time series tuple data.
	/// </summary>
	public class Indicator
	{
		public Indicator()
		{
			Series = new List<IndicatorTuple>();
		}

		public Indicator(IndicatorTuple[] series)
		{
			Series = series.ToList();
		}

		public List<IndicatorTuple> Series { get; set; }

		public void Add(IndicatorTuple tuple)
		{
			Series.Add(tuple);
		}

		public IndicatorTuple[] Tuples
		{
			get { return this.Series.ToArray(); }
		}
	}
}