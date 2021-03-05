using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Niffler.Data.ATS
{
	public sealed class TradeBook : TableEntity
	{
		public TradeBook()
		{
		}

		public TradeBook(string workflowIDQueueDate, string symbolPositionTypeTime) : base(workflowIDQueueDate, symbolPositionTypeTime)
		{
		}

		public TradeBook(string workflowIDqueueDate) : base(workflowIDqueueDate, null)
		{
		}

		public void SetRowKey()
		{
			this.RowKey = Util.ConformedKey(this.Symbol, this.TimestampEST, this.PositionType);
		}

		public long WorkflowID { get; set; }
		public long ServiceQueueID { get; set; }
		public Guid OrderID { get; set; }
		public string PositionType { get; set; }

		[IgnoreProperty]
		public PositionType ePositionType
		{
			get { return (PositionType)Enum.Parse(typeof(PositionType), this.PositionType); }
			set { this.PositionType = value.ToString(); }
		}

		public string Symbol { get; set; }
		public double LastQuotePrice { get; set; } //(8,4)
		public double AvgFillPrice { get; set; }
		public double ProfitLoss { get; set; } = 0D;
		public long LastQuoteVolume { get; set; }
		public long CurrentVolume { get; set; }
		public long CurrentOBV { get; set; }
		public int? Quantity { get; set; }
		public double? Amount { get; set; } //(12,4)
		public string TimestampEST { get; set; } = Util.ConformedDateTimeEST;
		public DateTime? FillDateTime { get; set; }
	}

	public class TradeBookOps : IDisposable
	{
		private CloudTable table;

		public TradeBookOps()
		{
			table = TableManager.GetCloudTable(typeof(TradeBook));
		}

		public async Task Merge(TradeBook data) => await Merge(data, CancellationToken.None);

		public async Task Merge(TradeBook data, CancellationToken token)
		{
			TableOperation mergeOperation = TableOperation.InsertOrMerge(data);
			await table.ExecuteAsync(mergeOperation, TableManager.TableOptions, null, token);
		}

		public async Task MergeBatch(IEnumerable<TradeBook> data) => await MergeBatch(data, CancellationToken.None);

		public async Task MergeBatch(IEnumerable<TradeBook> data, CancellationToken token)
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
						//Log storage exception: ex.RequestInformation.ExtendedErrorInformation.ErrorMessage
						throw ex;
					};
				}
			}
		}

		public async Task<List<TradeBook>> GetAllByKey(long workflowID, DateTime queueDate) => await GetAllByKey(workflowID, queueDate, CancellationToken.None);

		public async Task<List<TradeBook>> GetAllByKey(long workflowID, DateTime queueDate, CancellationToken token)
		{
			string pkFilter = TableQuery.GenerateFilterCondition(nameof(TableEntityField.PartitionKey), QueryComparisons.Equal, $"{workflowID}:{Util.ConformedDateString(queueDate)}");
			return await GetByFilter(pkFilter, token);
		}

		public async Task<List<TradeBook>> GetAllByKeyAndServiceQueue(long workflowID, DateTime queueDate, long serviceQueueID) => await GetAllByKeyAndServiceQueue(workflowID, queueDate, serviceQueueID, CancellationToken.None);

		public async Task<List<TradeBook>> GetAllByKeyAndServiceQueue(long workflowID, DateTime queueDate, long serviceQueueID, CancellationToken token)
		{
			string pkFilter = TableQuery.GenerateFilterCondition(nameof(TableEntityField.PartitionKey), QueryComparisons.Equal, $"{workflowID}:{Util.ConformedDateString(queueDate)}");
			string serviceQueueFilter = TableQuery.GenerateFilterConditionForLong(nameof(TableEntityField.ServiceQueueID), QueryComparisons.Equal, serviceQueueID);
			string combinedFilter = TableQuery.CombineFilters(pkFilter, TableOperators.And, serviceQueueFilter);
			return await GetByFilter(combinedFilter, token);
		}

		private async Task<List<TradeBook>> GetByFilter(string filter, CancellationToken token)
		{
			TableQuery<TradeBook> query = new TableQuery<TradeBook>();
			if (filter != null)
			{
				query.FilterString = filter;
			}
			List<TradeBook> result = new List<TradeBook>();
			TableContinuationToken continuationToken = null;
			do
			{
				TableQuerySegment<TradeBook> tableQueryResult =
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

		~TradeBookOps()
		{
			Dispose(false);
		}

		private bool disposed = false;

		#endregion IDisposable Overrides
	}
}