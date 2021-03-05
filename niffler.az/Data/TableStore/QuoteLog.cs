using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Niffler.Data.ATS
{
	public sealed class QuoteLog : TableEntity
	{
		public QuoteLog()
		{
		}

		public QuoteLog(string workflowIDqueueDate, string symbolTime) : base(workflowIDqueueDate, symbolTime)
		{
		}

		public QuoteLog(string workflowIDqueueDate) : base(workflowIDqueueDate, null)
		{
		}

		public void SetRowKey()
		{
			this.RowKey = Util.ConformedKey(this.Symbol, TimeStampRecorded.TimeOfDay);
			this.TimestampEST = this.TimeStampRecorded.ToString();
		}

		public long WorkflowID { get; set; }
		public long ServiceQueueID { get; set; }
		public string Symbol { get; set; }
		public string TimestampEST { get; set; }

		[IgnoreProperty]
		public DateTime TimeStampRecorded { get; set; }

		public double PriceClose { get; set; }
		public double? PriceOpen { get; set; }
		public double? PriceHigh { get; set; }
		public double? PriceLow { get; set; }
		public long? Volume { get; set; }
	}

	public class QuoteLogOps : IDisposable
	{
		private CloudTable table;

		public QuoteLogOps()
		{
			table = TableManager.GetCloudTable(typeof(QuoteLog));
		}

		public async Task Merge(QuoteLog data) => await Merge(data, CancellationToken.None);

		public async Task Merge(QuoteLog data, CancellationToken token)
		{
			TableOperation mergeOperation = TableOperation.InsertOrMerge(data);
			await table.ExecuteAsync(mergeOperation, TableManager.TableOptions, null, token);
		}

		public async Task MergeBatch(IEnumerable<QuoteLog> data) => await MergeBatch(data, CancellationToken.None);

		public async Task MergeBatch(IEnumerable<QuoteLog> data, CancellationToken token)
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
					try
					{
						await table.ExecuteBatchAsync(batchOperation, TableManager.TableOptions, null, token);
						counter = 0;
						batchOperation.Clear();
					}
					catch (StorageException ex)
					{
						//Log storage exception.
						throw ex;
					};
				};
			};
		}

		public async Task<List<QuoteLog>> GetAll() => await GetAll(CancellationToken.None);

		public async Task<List<QuoteLog>> GetAll(CancellationToken token) => await GetByFilter(null, CancellationToken.None);

		public async Task<List<QuoteLog>> GetAllByKey(long workflowID, DateTime queueDate) => await GetAllByKey(workflowID, queueDate, CancellationToken.None);

		public async Task<List<QuoteLog>> GetAllByKey(long workflowID, DateTime queueDate, CancellationToken token)
		{
			string pkFilter = TableQuery.GenerateFilterCondition(nameof(TableEntityField.PartitionKey), QueryComparisons.Equal, $"{workflowID}:{Util.ConformedDateString(queueDate)}");
			return await GetByFilter(pkFilter, token);
		}

		public async Task<List<QuoteLog>> GetAllByKeyAndServiceQueue(long workflowID, DateTime queueDate, long serviceQueueID) => await GetAllByKeyAndServiceQueue(workflowID, queueDate, serviceQueueID, CancellationToken.None);

		public async Task<List<QuoteLog>> GetAllByKeyAndServiceQueue(long workflowID, DateTime queueDate, long serviceQueueID, CancellationToken token)
		{
			string pkFilter = TableQuery.GenerateFilterCondition(nameof(TableEntityField.PartitionKey), QueryComparisons.Equal, $"{workflowID}:{Util.ConformedDateString(queueDate)}");
			string serviceQueueFilter = TableQuery.GenerateFilterConditionForLong(nameof(TableEntityField.ServiceQueueID), QueryComparisons.Equal, serviceQueueID);
			string combinedFilter = TableQuery.CombineFilters(pkFilter, TableOperators.And, serviceQueueFilter);
			return await GetByFilter(combinedFilter, token);
		}

		private async Task<List<QuoteLog>> GetByFilter(string filter, CancellationToken token)
		{
			TableQuery<QuoteLog> query = new TableQuery<QuoteLog>(); //.Where(filter);
			if (filter != null)
			{
				query.FilterString = filter;
			}
			List<QuoteLog> result = new List<QuoteLog>();
			TableContinuationToken continuationToken = null;
			do
			{
				TableQuerySegment<QuoteLog> tableQueryResult =
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

		~QuoteLogOps()
		{
			Dispose(false);
		}

		private bool disposed = false;

		#endregion IDisposable Overrides
	}
}