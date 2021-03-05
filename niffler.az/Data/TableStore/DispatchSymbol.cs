using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Niffler.Data.ATS
{
	public sealed class DispatchSymbol : TableEntity
	{
		public DispatchSymbol()
		{
		}

		public DispatchSymbol(string tradeDateAccountID, string symbol, bool shortable) : base(tradeDateAccountID, symbol)
		{
			this.Shortable = shortable;
		}

		public bool Shortable { get; set; }
	}

	public class DispatchSymbolOps : IDisposable
	{
		private CloudTable table;

		public DispatchSymbolOps()
		{
			table = TableManager.GetCloudTable(typeof(DispatchSymbol));
		}

		public async Task Merge(DispatchSymbol data) => await Merge(data, CancellationToken.None);

		public async Task Merge(DispatchSymbol data, CancellationToken token)
		{
			TableOperation mergeOperation = TableOperation.InsertOrMerge(data);
			await table.ExecuteAsync(mergeOperation, TableManager.TableOptions, null, token);
		}

		public async Task MergeBatch(IEnumerable<DispatchSymbol> data) => await MergeBatch(data, CancellationToken.None);

		public async Task MergeBatch(IEnumerable<DispatchSymbol> data, CancellationToken token)
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

		public async Task<List<DispatchSymbol>> GetAllByKey(DateTime queueDate, long accountID) => await GetAllByKey(queueDate, accountID, CancellationToken.None);

		public async Task<List<DispatchSymbol>> GetAllByKey(DateTime queueDate, long accountID, CancellationToken token)
		{
			string pkFilter = TableQuery.GenerateFilterCondition(nameof(TableEntityField.PartitionKey), QueryComparisons.Equal, $"{Util.ConformedDateString(queueDate)}:{accountID}");
			return await GetByFilter(pkFilter, token);
		}

		public async Task<List<DispatchSymbol>> GetAllByTradeDate(DateTime queueDate) => await GetAllByTradeDate(queueDate, CancellationToken.None);

		public async Task<List<DispatchSymbol>> GetAllByTradeDate(DateTime queueDate, CancellationToken token)
		{
			string pkFilter = TableQuery.GenerateFilterCondition(nameof(TableEntityField.PartitionKey), QueryComparisons.GreaterThan, $"{Util.ConformedDateString(queueDate)}");
			return await GetByFilter(pkFilter, token);
		}

		private async Task<List<DispatchSymbol>> GetByFilter(string filter, CancellationToken token)
		{
			TableQuery<DispatchSymbol> query = new TableQuery<DispatchSymbol>();
			if (filter != null)
			{
				query.FilterString = filter;
			}
			List<DispatchSymbol> result = new List<DispatchSymbol>();
			TableContinuationToken continuationToken = null;
			do
			{
				TableQuerySegment<DispatchSymbol> tableQueryResult =
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

		~DispatchSymbolOps()
		{
			Dispose(false);
		}

		private bool disposed = false;

		#endregion IDisposable Overrides
	}
}