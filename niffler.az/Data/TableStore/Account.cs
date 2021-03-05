using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Niffler.Data.ATS
{
	public sealed class Account : TableEntity
	{
		public Account()
		{
		}

		public Account(string accountID) : base(accountID, accountID)
		{
		}

		public long AccountID { get; set; }
		public string BrokerageName { get; set; }
		public string BrokerageAccountID { get; set; }
		public string Description { get; set; }
		public bool Notify { get; set; }
		public string NotifyRecipient { get; set; }
		public double PrincipalBalance { get; set; } //(11,2)
		public double? CurrentBalance { get; set; } //(11,2)
		public DateTime? LastUpdated { get; set; }
	}

	public class AccountOps : IDisposable
	{
		private CloudTable table;

		public AccountOps()
		{
			table = TableManager.GetCloudTable(typeof(Account));
		}

		public async Task Merge(Account data) => await Merge(data, CancellationToken.None);

		public async Task Merge(Account data, CancellationToken token)
		{
			TableOperation mergeOperation = TableOperation.InsertOrMerge(data);
			await table.ExecuteAsync(mergeOperation, TableManager.TableOptions, null, token);
		}

		public async Task MergeBatch(IEnumerable<Account> data) => await MergeBatch(data, CancellationToken.None);

		public async Task MergeBatch(IEnumerable<Account> data, CancellationToken token)
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

		public async Task<List<Account>> GetAll() => await GetAll(CancellationToken.None);

		public async Task<List<Account>> GetAll(CancellationToken token)
		{
			TableQuery<Account> query = new TableQuery<Account>();
			List<Account> result = new List<Account>();
			TableContinuationToken continuationToken = null;
			do
			{
				TableQuerySegment<Account> tableQueryResult =
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

		~AccountOps()
		{
			Dispose(false);
		}

		private bool disposed = false;

		#endregion IDisposable Overrides
	}
}