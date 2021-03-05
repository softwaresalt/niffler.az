using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Niffler.Data.ATS
{
	public sealed class ServiceQueue : TableEntity
	{
		public ServiceQueue()
		{
		}

		public ServiceQueue(string queueDate, string workflowID) : base(queueDate, workflowID)
		{
		}

		public ServiceQueue(ServiceQueueDTO.TaskDTO task) : base(Util.ConformedDateString(task.QueueDate), task.WorkflowID.ToString())
		{
			this.QueueDate = task.QueueDate;
			this.QueueItem = task.SerializeQueueItem();
			this.WorkflowID = task.WorkflowID;
			this.QueueTime = Util.ConformedTimeString(task.QueueTime);
			this.ServiceQueueID = task.ServiceQueueID;
			this.StartTimeEST = Util.ConformedTimeString(task.StartTimeEST);
			this.RunByTimeEST = Util.ConformedTimeString(task.RunByTimeEST);
		}

		public long ServiceQueueID { get; set; }
		public long WorkflowID { get; set; }
		public DateTime QueueDate { get; set; }
		public string StartTimeEST { get; set; } //TimeSpan.TotalSeconds
		public string RunByTimeEST { get; set; } //TimeSpan.TotalSeconds
		public string QueueItem { get; set; }
		public string QueueTime { get; set; } //TimeSpan.TotalSeconds
		public DateTime? CompletionDate { get; set; }
		public bool IsCancelled { get; set; } = false;
	}

	public class ServiceQueueOps : IDisposable
	{
		private CloudTable table;

		public ServiceQueueOps()
		{
			table = TableManager.GetCloudTable(typeof(ServiceQueue));
		}

		public async Task Merge(ServiceQueue data) => await Merge(data, CancellationToken.None);

		public async Task Merge(ServiceQueue data, CancellationToken token)
		{
			TableOperation mergeOperation = TableOperation.InsertOrMerge(data);
			await table.ExecuteAsync(mergeOperation, TableManager.TableOptions, null, token);
		}

		public async Task MergeBatch(IEnumerable<ServiceQueue> data) => await MergeBatch(data, CancellationToken.None);

		public async Task MergeBatch(IEnumerable<ServiceQueue> data, CancellationToken token)
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

		public async Task<List<ServiceQueue>> GetAllEnabledByKey(string queueDate) => await GetAllEnabledByKey(queueDate, CancellationToken.None);

		public async Task<List<ServiceQueue>> GetAllEnabledByKey(string queueDate, CancellationToken token)
		{
			string pkFilter = TableQuery.GenerateFilterCondition(nameof(TableEntityField.PartitionKey), QueryComparisons.Equal, queueDate);
			string enabledFilter = TableQuery.GenerateFilterConditionForBool(nameof(TableEntityField.IsCancelled), QueryComparisons.Equal, false);
			string combinedFilter = TableQuery.CombineFilters(pkFilter, TableOperators.And, enabledFilter);
			return await GetByFilter(combinedFilter, token);
		}

		private async Task<List<ServiceQueue>> GetByFilter(string filter, CancellationToken token)
		{
			TableQuery<ServiceQueue> query = new TableQuery<ServiceQueue>();
			if (filter != null)
			{
				query.FilterString = filter;
			}
			List<ServiceQueue> result = new List<ServiceQueue>();
			TableContinuationToken continuationToken = null;
			do
			{
				TableQuerySegment<ServiceQueue> tableQueryResult =
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

		~ServiceQueueOps()
		{
			Dispose(false);
		}

		private bool disposed = false;

		#endregion IDisposable Overrides
	}
}