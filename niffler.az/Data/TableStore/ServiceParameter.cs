using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Niffler.Data.ATS
{
	public sealed class ServiceParameter : TableEntity
	{
		public ServiceParameter()
		{
		}

		public ServiceParameter(string serviceID, string parameterIDVersion) : base(serviceID, parameterIDVersion)
		{
		}

		public long ParameterID { get; set; }
		public long ServiceID { get; set; }
		public string Name { get; set; }
		public int Version { get; set; }
		public int Limit { get; set; }
		public string QueryString { get; set; }
		public bool IsEnabled { get; set; }
	}

	public class ServiceParameterOps : IDisposable
	{
		private CloudTable table;

		public ServiceParameterOps()
		{
			table = TableManager.GetCloudTable(typeof(ServiceParameter));
		}

		public async Task Merge(ServiceParameter data) => await Merge(data, CancellationToken.None);

		public async Task Merge(ServiceParameter data, CancellationToken token)
		{
			TableOperation mergeOperation = TableOperation.InsertOrMerge(data);
			await table.ExecuteAsync(mergeOperation, TableManager.TableOptions, null, token);
		}

		public async Task MergeBatch(IEnumerable<ServiceParameter> data) => await MergeBatch(data, CancellationToken.None);

		public async Task MergeBatch(IEnumerable<ServiceParameter> data, CancellationToken token)
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

		public async Task<List<ServiceParameter>> GetAllEnabledByKey(string serviceID) => await GetAllEnabledByKey(serviceID, CancellationToken.None);

		public async Task<List<ServiceParameter>> GetAllEnabledByKey(string serviceID, CancellationToken token)
		{
			string pkFilter = TableQuery.GenerateFilterCondition(nameof(TableEntityField.PartitionKey), QueryComparisons.Equal, serviceID);
			string enabledFilter = TableQuery.GenerateFilterConditionForBool(nameof(TableEntityField.IsEnabled), QueryComparisons.Equal, true);
			string combinedFilter = TableQuery.CombineFilters(pkFilter, TableOperators.And, enabledFilter);
			TableQuery<ServiceParameter> query = new TableQuery<ServiceParameter>().Where(combinedFilter);
			List<ServiceParameter> result = new List<ServiceParameter>();
			TableContinuationToken continuationToken = null;
			do
			{
				TableQuerySegment<ServiceParameter> tableQueryResult =
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

		~ServiceParameterOps()
		{
			Dispose(false);
		}

		private bool disposed = false;

		#endregion IDisposable Overrides
	}
}