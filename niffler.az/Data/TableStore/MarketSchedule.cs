using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Niffler.Data.ATS
{
	public sealed class MarketSchedule : TableEntity
	{
		public MarketSchedule()
		{
		}

		public MarketSchedule(string scheduleDate) : base(scheduleDate, scheduleDate)
		{
		}

		public string MarketHoliday { get; set; }
		public DateTime ScheduleDate { get; set; }
		public string EarlyCloseTimeEST { get; set; } //TimeSpan.TotalSeconds
		public bool IsClosed { get; set; }
	}

	public class MarketScheduleOps : IDisposable
	{
		private CloudTable table;

		public MarketScheduleOps()
		{
			table = TableManager.GetCloudTable(typeof(MarketSchedule));
		}

		public async Task Merge(MarketSchedule data) => await Merge(data, CancellationToken.None);

		public async Task Merge(MarketSchedule data, CancellationToken token)
		{
			TableOperation mergeOperation = TableOperation.InsertOrMerge(data);
			await table.ExecuteAsync(mergeOperation, TableManager.TableOptions, null, token);
		}

		public async Task MergeBatch(IEnumerable<MarketSchedule> data) => await MergeBatch(data, CancellationToken.None);

		public async Task MergeBatch(IEnumerable<MarketSchedule> data, CancellationToken token)
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

		public async Task<List<MarketSchedule>> GetAllByKey(string scheduleDate) => await GetAllByKey(scheduleDate, CancellationToken.None);

		public async Task<List<MarketSchedule>> GetAllByKey(string scheduleDate, CancellationToken token)
		{
			string pkFilter = TableQuery.GenerateFilterCondition(nameof(TableEntityField.PartitionKey), QueryComparisons.Equal, scheduleDate);
			return await GetByFilter(pkFilter, token);
		}

		private async Task<List<MarketSchedule>> GetByFilter(string filter, CancellationToken token)
		{
			TableQuery<MarketSchedule> query = new TableQuery<MarketSchedule>();
			if (filter != null)
			{
				query.FilterString = filter;
			}
			List<MarketSchedule> result = new List<MarketSchedule>();
			TableContinuationToken continuationToken = null;
			do
			{
				TableQuerySegment<MarketSchedule> tableQueryResult =
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

		~MarketScheduleOps()
		{
			Dispose(false);
		}

		private bool disposed = false;

		#endregion IDisposable Overrides
	}
}