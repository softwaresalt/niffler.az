using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Niffler.Data.ATS
{
	public sealed class WorkflowOperation : TableEntity
	{
		public WorkflowOperation()
		{
		}

		public WorkflowOperation(string workflowID, string serviceOperationType) : base(workflowID, serviceOperationType)
		{
		}

		public long WorkflowID { get; set; }
		public long ServiceID { get; set; }
		public long? ParameterID { get; set; }
		public string ServiceType { get; set; }

		[IgnoreProperty]
		public ServiceType ServiceTypeEnum
		{
			get { return (ServiceType)Enum.Parse(typeof(ServiceType), this.ServiceType); }
			set { this.ServiceType = value.ToString(); }
		}

		public string OperationType { get; set; }

		[IgnoreProperty]
		public OperationType OperationTypeEnum
		{
			get { return (OperationType)Enum.Parse(typeof(OperationType), this.OperationType); }
			set { this.OperationType = value.ToString(); }
		}

		public int Ordinal { get; set; }
		public int Limit { get; set; }
		public int? Minimum { get; set; }
		public string Sequence { get; set; }
		public string StartTimeEST { get; set; } //TimeSpan.TotalSeconds
		public string RunByTimeEST { get; set; } //TimeSpan.TotalSeconds
		public bool IsEnabled { get; set; }
	}

	public class WorkflowOperationOps : IDisposable
	{
		private CloudTable table;

		public WorkflowOperationOps()
		{
			table = TableManager.GetCloudTable(typeof(WorkflowOperation));
		}

		public async Task Merge(WorkflowOperation data) => await Merge(data, CancellationToken.None);

		public async Task Merge(WorkflowOperation data, CancellationToken token)
		{
			TableOperation mergeOperation = TableOperation.InsertOrMerge(data);
			await table.ExecuteAsync(mergeOperation, TableManager.TableOptions, null, token);
		}

		public async Task MergeBatch(IEnumerable<WorkflowOperation> data) => await MergeBatch(data, CancellationToken.None);

		public async Task MergeBatch(IEnumerable<WorkflowOperation> data, CancellationToken token)
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

		public async Task<List<WorkflowOperation>> GetAllEnabled() => await GetAllEnabled(CancellationToken.None);

		public async Task<List<WorkflowOperation>> GetAllEnabled(CancellationToken token)
		{
			string enabledFilter = TableQuery.GenerateFilterConditionForBool(nameof(TableEntityField.IsEnabled), QueryComparisons.Equal, true);
			return await GetByFilter(enabledFilter, token);
		}

		public async Task<List<WorkflowOperation>> GetAllEnabledByKey(string workflowID) => await GetAllEnabledByKey(workflowID, CancellationToken.None);

		public async Task<List<WorkflowOperation>> GetAllEnabledByKey(string workflowID, CancellationToken token)
		{
			string pkFilter = TableQuery.GenerateFilterCondition(nameof(TableEntityField.PartitionKey), QueryComparisons.Equal, workflowID);
			string enabledFilter = TableQuery.GenerateFilterConditionForBool(nameof(TableEntityField.IsEnabled), QueryComparisons.Equal, true);
			string combinedFilter = TableQuery.CombineFilters(pkFilter, TableOperators.And, enabledFilter);
			return await GetByFilter(combinedFilter, token);
		}

		private async Task<List<WorkflowOperation>> GetByFilter(string filter, CancellationToken token)
		{
			TableQuery<WorkflowOperation> query = new TableQuery<WorkflowOperation>();
			if (filter != null)
			{
				query.FilterString = filter;
			}
			List<WorkflowOperation> result = new List<WorkflowOperation>();
			TableContinuationToken continuationToken = null;
			do
			{
				TableQuerySegment<WorkflowOperation> tableQueryResult =
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

		~WorkflowOperationOps()
		{
			Dispose(false);
		}

		private bool disposed = false;

		#endregion IDisposable Overrides
	}
}