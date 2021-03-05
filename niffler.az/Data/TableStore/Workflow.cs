using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Niffler.Data.ATS
{
	public sealed class Workflow : TableEntity
	{
		public Workflow()
		{
		}

		public Workflow(string accountID, string workflowID) : base(accountID, workflowID)
		{
		}

		public long AccountID { get; set; } //PartitionKey
		public long WorkflowID { get; set; } //RowKey
		public string Name { get; set; }
		public string Description { get; set; }
		public string StartTimeEST { get; set; } //TimeSpan.TotalSeconds
		public string RunByTimeEST { get; set; } //TimeSpan.TotalSeconds
		public string ModuleType { get; set; }

		[IgnoreProperty]
		public ModuleType ModuleTypeEnum
		{
			get { return (ModuleType)Enum.Parse(typeof(ModuleType), this.ModuleType); }
			set { this.ModuleType = value.ToString(); }
		}

		public double PrincipalBalance { get; set; } //(11,2) NOT NULL,
		public double CurrentBalance { get; set; } //(11,2) NOT NULL,
		public bool IsEnabled { get; set; }
		public DateTime LastUpdated { get; set; } = Util.CurrentDateTimeEST;
	}

	public class WorkflowOps : IDisposable
	{
		private CloudTable table;

		public WorkflowOps()
		{
			table = TableManager.GetCloudTable(typeof(Workflow));
		}

		public async Task Merge(Workflow data) => await Merge(data, CancellationToken.None);

		public async Task Merge(Workflow data, CancellationToken token)
		{
			TableOperation mergeOperation = TableOperation.InsertOrMerge(data);
			await table.ExecuteAsync(mergeOperation, TableManager.TableOptions, null, token);
		}

		public async Task MergeBatch(IEnumerable<Workflow> data) => await MergeBatch(data, CancellationToken.None);

		public async Task MergeBatch(IEnumerable<Workflow> data, CancellationToken token)
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

		public async Task<List<Workflow>> GetAllEnabledByKey(string accountID) => await GetAllEnabledByKey(accountID, CancellationToken.None);

		public async Task<List<Workflow>> GetAllEnabledByKey(string accountID, CancellationToken token)
		{
			string pkFilter = TableQuery.GenerateFilterCondition(nameof(TableEntityField.PartitionKey), QueryComparisons.Equal, accountID);
			string enabledFilter = TableQuery.GenerateFilterConditionForBool(nameof(TableEntityField.IsEnabled), QueryComparisons.Equal, true);
			string combinedFilter = TableQuery.CombineFilters(pkFilter, TableOperators.And, enabledFilter);
			return await GetByFilter(combinedFilter, token);
		}

		private async Task<List<Workflow>> GetByFilter(string filter, CancellationToken token)
		{
			TableQuery<Workflow> query = new TableQuery<Workflow>();
			if (filter != null)
			{
				query.FilterString = filter;
			}
			List<Workflow> result = new List<Workflow>();
			TableContinuationToken continuationToken = null;
			do
			{
				TableQuerySegment<Workflow> tableQueryResult =
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

		~WorkflowOps()
		{
			Dispose(false);
		}

		private bool disposed = false;

		#endregion IDisposable Overrides
	}
}