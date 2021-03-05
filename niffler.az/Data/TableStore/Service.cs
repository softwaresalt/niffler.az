using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Niffler.Data.ATS
{
	public sealed class Service : TableEntity
	{
		public Service()
		{
		}

		public Service(string serviceID) : base(serviceID, serviceID)
		{
		}

		public long ServiceID { get; set; }
		public string Name { get; set; }
		public string ServiceType { get; set; }

		[IgnoreProperty]
		public ServiceType ServiceTypeEnum
		{
			get { return (ServiceType)Enum.Parse(typeof(ServiceType), this.ServiceType); }
			set { this.ServiceType = value.ToString(); }
		}

		public string URL { get; set; }
		public bool IsEnabled { get; set; }
	}

	public class ServiceOps : IDisposable
	{
		private CloudTable table;

		public ServiceOps()
		{
			table = TableManager.GetCloudTable(typeof(Service));
		}

		public async Task Merge(Service data) => await Merge(data, CancellationToken.None);

		public async Task Merge(Service data, CancellationToken token)
		{
			TableOperation mergeOperation = TableOperation.InsertOrMerge(data);
			await table.ExecuteAsync(mergeOperation, TableManager.TableOptions, null, token);
		}

		public async Task MergeBatch(IEnumerable<Service> data) => await MergeBatch(data, CancellationToken.None);

		public async Task MergeBatch(IEnumerable<Service> data, CancellationToken token)
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

		public async Task<List<Service>> GetAllEnabled() => await GetAllEnabled(CancellationToken.None);

		public async Task<List<Service>> GetAllEnabled(CancellationToken token)
		{
			string enabledFilter = TableQuery.GenerateFilterConditionForBool(nameof(TableEntityField.IsEnabled), QueryComparisons.Equal, true);
			TableQuery<Service> query = new TableQuery<Service>().Where(enabledFilter);
			List<Service> result = new List<Service>();
			TableContinuationToken continuationToken = null;
			do
			{
				TableQuerySegment<Service> tableQueryResult =
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

		~ServiceOps()
		{
			Dispose(false);
		}

		private bool disposed = false;

		#endregion IDisposable Overrides
	}
}