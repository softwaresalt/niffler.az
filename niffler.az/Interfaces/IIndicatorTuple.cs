using System;

namespace Niffler.Interfaces
{
	internal interface IIndicatorTuple
	{
		//Time series data point
		DateTime Time { get; set; }

		//Quantitative Data Point (Datum)
		double? QDatum { get; set; }
	}
}