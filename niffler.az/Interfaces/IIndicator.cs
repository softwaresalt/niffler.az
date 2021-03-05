using Niffler.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Niffler.Interfaces
{
	/// <summary>
	/// Abstract indicator to be used by all indicator instances
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class VIndicator<T>
	{
		public virtual IndicatorTuple[] Tuples { get; set; }
		public virtual IndicatorTuple[] BaseTuples { get; set; }
		public virtual Indicator TupleList { get { return new Indicator(Tuples); } }
		public virtual Quote[] Quotes { get; set; }
		public virtual List<Quote> QuoteList { get { return this.Quotes.ToList(); } }
		protected virtual int Period { get; set; } = 7;
		protected virtual int Iterations { get; set; } = 3;
		protected virtual bool Wilder { get; set; } = false;
		protected virtual bool IsQuotes { get; set; } = true;
		protected virtual bool CalculatePercentage { get; set; } = true;
		protected virtual double Factor { get; set; } = 2;
		protected virtual bool UsePeriodChangeTime { get; set; } = false;
		protected virtual bool UseVolume { get; set; } = false;
		protected virtual TimeSpan PeriodChangeTime { get; set; } = new TimeSpan(10, 30, 00);
		protected virtual int PeriodChangeAmount { get; set; } = 0;

		public abstract T Calculate();

		public virtual void LoadSeries(List<Quote> quoteList, QuoteField field = QuoteField.Close)
		{
			if (this.Quotes != null) { this.Quotes = null; }
			if (this.Tuples != null) { this.Tuples = null; }
			this.Quotes = quoteList.ConvertAll(q => new Quote(q)).ToArray(); //Make deep copy to be safe
			switch (field)
			{
				case QuoteField.Open:
					this.Tuples = quoteList.ConvertAll(t => new IndicatorTuple(t.Time, t.Open, t.Volume)).ToArray(); //Make deep copy to be safe
					break;

				case QuoteField.High:
					this.Tuples = quoteList.ConvertAll(t => new IndicatorTuple(t.Time, t.High, t.Volume)).ToArray(); //Make deep copy to be safe
					break;

				case QuoteField.Low:
					this.Tuples = quoteList.ConvertAll(t => new IndicatorTuple(t.Time, t.Low, t.Volume)).ToArray(); //Make deep copy to be safe
					break;

				case QuoteField.Close:
					this.Tuples = quoteList.ConvertAll(t => new IndicatorTuple(t.Time, t.Close, t.Volume)).ToArray(); //Make deep copy to be safe
					break;
			}
		}

		public virtual void LoadSeries(Quote[] quotes, QuoteField field = QuoteField.Close)
		{
			LoadSeries(quotes.ToList(), field);
		}

		public virtual void LoadSeries(List<IndicatorTuple> tupleList, bool isQuotes = true, bool isBaseTuples = false)
		{
			if (this.Tuples != null) { this.Tuples = null; }
			this.IsQuotes = isQuotes;
			if (isBaseTuples)
			{
				this.BaseTuples = tupleList.ConvertAll(t => new IndicatorTuple(t.Time, t.QDatum, t.Volume)).ToArray(); //Make deep copy to be safe
			}
			else
			{
				this.Tuples = tupleList.ConvertAll(t => new IndicatorTuple(t.Time, t.QDatum, t.Volume)).ToArray(); //Make deep copy to be safe
			}
		}

		public virtual void LoadSeries(IndicatorTuple[] tuples, bool isQuotes = true)
		{
			LoadSeries(tuples.ToList(), isQuotes);
		}

		public enum QuoteField
		{
			Open,
			High,
			Low,
			Close
		}
	}
}