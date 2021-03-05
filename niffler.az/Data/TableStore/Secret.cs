using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Niffler.Data.ATS
{
	public sealed class Secret : TableEntity
	{
		public Secret()
		{
		}

		public Secret(string accountID, string secretType) : base(accountID, secretType)
		{
		}

		public long AccountID
		{
			get { return long.Parse(this.PartitionKey); }
			set { this.PartitionKey = value.ToString(); }
		}

		[IgnoreProperty]
		public SecretType eSecretType
		{
			get { return (SecretType)Enum.Parse(typeof(SecretType), this.RowKey); }
			set { this.RowKey = value.ToString(); }
		}

		public string KeyName { get; set; }
	}

	public class SecretOps : IDisposable
	{
		private CloudTable table;

		public SecretOps()
		{
			table = TableManager.GetCloudTable(typeof(Secret));
		}

		public async Task Merge(Secret data) => await Merge(data, CancellationToken.None);

		public async Task Merge(Secret data, CancellationToken token)
		{
			TableOperation mergeOperation = TableOperation.InsertOrMerge(data);
			await table.ExecuteAsync(mergeOperation, TableManager.TableOptions, null, token);
		}

		public async Task MergeBatch(IEnumerable<Secret> data) => await MergeBatch(data, CancellationToken.None);

		public async Task MergeBatch(IEnumerable<Secret> data, CancellationToken token)
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

		public async Task<List<Secret>> GetAllByKey(long accountID) => await GetAllByKey(accountID, CancellationToken.None);

		public async Task<List<Secret>> GetAllByKey(long accountID, CancellationToken token)
		{
			string pkFilter = TableQuery.GenerateFilterCondition(nameof(TableEntityField.PartitionKey), QueryComparisons.Equal, $"{accountID}");
			return await GetByFilter(pkFilter, token);
		}

		public async Task<List<Secret>> GetAll() => await GetAll(CancellationToken.None);

		public async Task<List<Secret>> GetAll(CancellationToken token)
		{
			TableQuery<Secret> query = new TableQuery<Secret>();
			List<Secret> result = new List<Secret>();
			TableContinuationToken continuationToken = null;
			do
			{
				TableQuerySegment<Secret> tableQueryResult =
					await table.ExecuteQuerySegmentedAsync(query, continuationToken, TableManager.TableOptions, null, token);

				result.AddRange(tableQueryResult.Results);
			} while (continuationToken != null);

			return result;
		}

		private async Task<List<Secret>> GetByFilter(string filter, CancellationToken token)
		{
			TableQuery<Secret> query = new TableQuery<Secret>();
			if (filter != null)
			{
				query.FilterString = filter;
			}
			List<Secret> result = new List<Secret>();
			TableContinuationToken continuationToken = null;
			do
			{
				TableQuerySegment<Secret> tableQueryResult =
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

		~SecretOps()
		{
			Dispose(false);
		}

		private bool disposed = false;

		#endregion IDisposable Overrides
	}
}