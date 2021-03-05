using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// ATS: Azure Table Storage
/// </summary>
namespace Niffler.Data.ATS
{
	public sealed class ScreenLog : TableEntity
	{
		public ScreenLog()
		{
		}

		public ScreenLog(string workflowIDqueueDate, string symbol) : base(workflowIDqueueDate, symbol)
		{
		}

		public ScreenLog(string workflowIDqueueDate) : base(workflowIDqueueDate, null)
		{
		}

		public void SetRowKey()
		{
			this.RowKey = this.Symbol;
		}

		public long WorkflowID { get; set; }
		public long ServiceQueueID { get; set; }
		public int Ordinal { get; set; }
		public string Symbol { get; set; }
		public string Company { get; set; }
		public string Sector { get; set; }
		public string Industry { get; set; }
		public string Country { get; set; }
		public double MarketCap { get; set; } //(12, 2)
		public double PE { get; set; } //(6, 2)
		public double? FPE { get; set; } //(6, 2)
		public double? PEG { get; set; } //(6, 2)
		public double? PS { get; set; } //(6, 2)
		public double? PB { get; set; } //(6, 2)
		public double? PCash { get; set; } //(8, 2)
		public double? PFreeCashFlow { get; set; } //(6, 2)
		public double? DividendYield { get; set; } //(6, 2)
		public double? Payout { get; set; } //(6, 2)
		public double? EPS_ttm { get; set; } //(6, 2)
		public double? EPS_GTY { get; set; } //(6, 2)
		public double? EPS_GNY { get; set; } //(6, 2)
		public double? EPS_GP5Y { get; set; } //(6, 2)
		public double? EPS_GN5Y { get; set; } //(6, 2)
		public double? Sales_GP5Y { get; set; } //(6, 2)
		public double? EPS_GQoQ { get; set; } //(8, 2)
		public double? Sales_GQoQ { get; set; } //(6, 2)
		public double? SharesOut { get; set; } //(6, 2)
		public double? SharesFloat { get; set; } //(6, 2)
		public double? InsiderOwned { get; set; } //(4, 2)
		public double? InsiderTx { get; set; } //(6, 2)
		public double? InstOwn { get; set; } //(4, 2)
		public double? InstTx { get; set; } //(4, 2)
		public double? FloatShort { get; set; } //(4, 2)
		public double? ShortRatio { get; set; } //(4, 2)
		public double? ROA { get; set; } //(4, 2)
		public double? ROE { get; set; } //(4, 2)
		public double? ROI { get; set; } //(6, 2)
		public double? CurrentRatio { get; set; } //(3, 1)
		public double? QuickRatio { get; set; } //(3, 1)
		public double? LT_DE { get; set; } //(4, 2)
		public double? Total_DE { get; set; } //(4, 2)
		public double? GrossMargin { get; set; } //(4, 2)
		public double? OperationMargin { get; set; } //(4, 2)
		public double? ProfitMargin { get; set; } //(4, 2)
		public double? PerfByWeek { get; set; } //(4, 2)
		public double? PerfByMonth { get; set; } //(4, 2)
		public double? PerfByQuarter { get; set; } //(6, 2)
		public double? PerfByHalfYear { get; set; } //(6, 2)
		public double? PerfByYear { get; set; } //(6, 2)
		public double? PerfYTD { get; set; } //(6, 2)
		public double? Beta { get; set; } //(3, 2)
		public double? AvgTrueRange { get; set; } //(4, 2)
		public double? VolatilityByWeek { get; set; } //(4, 2)
		public double? VolatilityByMonth { get; set; } //(4, 2)
		public double? SMA20Day { get; set; } //(4, 2)
		public double? SMA50Day { get; set; } //(4, 2)
		public double? SMA200Day { get; set; } //(6, 2)
		public double? High50Day { get; set; } //(4, 2)
		public double? Low50Day { get; set; } //(4, 2)
		public double? High52Week { get; set; } //(6, 2)
		public double? Low52Week { get; set; } //(6, 2)
		public double? RSI14 { get; set; } //(4, 2)
		public double? CfO { get; set; } //(4, 2)
		public double? Gap { get; set; } //(4, 2)
		public double? AnalystRecommend { get; set; } //(3, 1)
		public double? AvgVol { get; set; } //(12, 2)
		public double? RelVol { get; set; } //(4, 2)
		public double? Price { get; set; } //(8, 2)
		public double? Change { get; set; } //(4, 2)
		public int? Volume { get; set; }
		public DateTime? EarningsDate { get; set; }
		public double? TargetPrice { get; set; } //(8, 2)
		public DateTime? IPODate { get; set; }
		public bool Shortable { get; set; }
	}

	public class ScreenLogOps : IDisposable
	{
		private CloudTable table { get; set; } = TableManager.GetCloudTable(typeof(ScreenLog));

		public ScreenLogOps()
		{
		}

		public async Task Merge(ScreenLog data) => await Merge(data, CancellationToken.None);

		public async Task Merge(ScreenLog data, CancellationToken token)
		{
			TableOperation mergeOperation = TableOperation.InsertOrMerge(data);
			await table.ExecuteAsync(mergeOperation, TableManager.TableOptions, null, token);
		}

		public async Task MergeBatch(IEnumerable<ScreenLog> data) => await MergeBatch(data, CancellationToken.None);

		public async Task MergeBatch(IEnumerable<ScreenLog> data, CancellationToken token)
		{
			TableBatchOperation batchOperation = new TableBatchOperation();
			int counter = 0;
			int batchSize = data.Count();

			foreach (var item in data)
			{
				TableOperation mergeOperation = TableOperation.InsertOrMerge(item);
				batchOperation.Add(mergeOperation);
				counter++;
				batchSize--;

				if (counter % 100 == 0 || batchSize == 0)
				{
					await table.ExecuteBatchAsync(batchOperation, TableManager.TableOptions, null, token);
					counter = 0;
					batchOperation.Clear();
				}
			}
		}

		public async Task<List<ScreenLog>> GetAllByKey(long workflowID, DateTime queueDate) => await GetAllByKey(workflowID, queueDate, CancellationToken.None);

		public async Task<List<ScreenLog>> GetAllByKey(long workflowID, DateTime queueDate, CancellationToken token)
		{
			string pkFilter = TableQuery.GenerateFilterCondition(nameof(TableEntityField.PartitionKey), QueryComparisons.Equal, $"{workflowID}:{Util.ConformedDateString(queueDate)}");
			return await GetByFilter(pkFilter, token);
		}

		private async Task<List<ScreenLog>> GetByFilter(string filter, CancellationToken token)
		{
			TableQuery<ScreenLog> query = new TableQuery<ScreenLog>();
			if (filter != null)
			{
				query.FilterString = filter;
			}
			List<ScreenLog> result = new List<ScreenLog>();
			TableContinuationToken continuationToken = null;
			do
			{
				TableQuerySegment<ScreenLog> tableQueryResult =
					await table.ExecuteQuerySegmentedAsync(query, continuationToken, TableManager.TableOptions, null, token);

				result.AddRange(tableQueryResult.Results);
			} while (continuationToken != null);

			return result;
		}

		#region IDisposable Overrides

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				disposed = true;
			}
		}

		~ScreenLogOps()
		{
			Dispose(false);
		}

		private bool disposed = false;

		#endregion IDisposable Overrides
	}
}