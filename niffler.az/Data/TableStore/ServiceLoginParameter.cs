using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Niffler.Data.ATS
{
	public class ServiceLoginParameter : TableEntity
	{
		public ServiceLoginParameter()
		{
		}

		public ServiceLoginParameter(string serviceID, string serviceLoginParameterType) : base(serviceID, serviceLoginParameterType)
		{
		}

		public long ServiceID { get; set; }
		public string ServiceLoginParameterType { get; set; }

		[IgnoreProperty]
		public ServiceLoginParameterType eServiceLoginParameterType
		{
			get { return (ServiceLoginParameterType)Enum.Parse(typeof(ServiceLoginParameterType), this.ServiceLoginParameterType); }
			set { this.ServiceLoginParameterType = nameof(value); this.eServiceLoginParameterType = value; }
		}

		public string ParameterValue { get; set; }
	}

	public class ServiceLoginParameterOps : IDisposable
	{
		private CloudTable table;

		public ServiceLoginParameterOps()
		{
			table = TableManager.GetCloudTable(typeof(ServiceLoginParameter));
		}

		public async Task Merge(ServiceLoginParameter data) => await Merge(data, CancellationToken.None);

		public async Task Merge(ServiceLoginParameter data, CancellationToken token)
		{
			TableOperation mergeOperation = TableOperation.InsertOrMerge(data);
			await table.ExecuteAsync(mergeOperation, TableManager.TableOptions, null, token);
		}

		public async Task MergeBatch(IEnumerable<ServiceLoginParameter> data) => await MergeBatch(data, CancellationToken.None);

		public async Task MergeBatch(IEnumerable<ServiceLoginParameter> data, CancellationToken token)
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

		public async Task<List<ServiceLoginParameter>> GetAllByKey(string serviceID) => await GetAllByKey(serviceID, CancellationToken.None);

		public async Task<List<ServiceLoginParameter>> GetAllByKey(string serviceID, CancellationToken token)
		{
			string pkFilter = TableQuery.GenerateFilterCondition(nameof(TableEntityField.PartitionKey), QueryComparisons.Equal, serviceID);
			TableQuery<ServiceLoginParameter> query = new TableQuery<ServiceLoginParameter>().Where(pkFilter);
			List<ServiceLoginParameter> result = new List<ServiceLoginParameter>();
			TableContinuationToken continuationToken = null;
			do
			{
				TableQuerySegment<ServiceLoginParameter> tableQueryResult =
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

		~ServiceLoginParameterOps()
		{
			Dispose(false);
		}

		private bool disposed = false;

		#endregion IDisposable Overrides
	}
}