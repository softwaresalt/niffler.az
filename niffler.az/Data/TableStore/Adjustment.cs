using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Niffler.Data.ATS
{
	//	AdjustmentID int IDENTITY(1,1) NOT NULL,
	//	AccountID int NULL,
	//	AccountKey char (20) NULL, --PartitionKey
	//	WorkflowID smallint NULL,
	//	WorkflowKey char (20) NULL, --RowKey
	//	AdjustmentType int NOT NULL,
	//	Amount decimal (11,2) NOT NULL,
	//	CONSTRAINT PK_Adjustment PRIMARY KEY(AdjustmentID),
	//	CONSTRAINT FK_Adjustment_WorkflowAccount FOREIGN KEY(WorkflowID) REFERENCES dbo.Workflow(WorkflowID),
	//	CONSTRAINT FK_Adjustment_Account FOREIGN KEY(AccountID) REFERENCES dbo.Account(AccountID)

	public class Adjustment : TableEntity
	{
		public Adjustment(string accountID, string workflowID) : base(accountID, workflowID)
		{
			this.PartitionKey = accountID;
			this.RowKey = workflowID;
		}

		public Adjustment()
		{
		}

		[JsonConverter(typeof(StringEnumConverter))]
		public string AdjustmentType { get; set; }

		public decimal Amount { get; set; }
	}

	public class AdjustmentOps
	{
		private CloudTable table;

		public AdjustmentOps()
		{
			table = TableManager.GetCloudTable(typeof(Adjustment));
		}

		public async Task Merge(Adjustment data) => await Merge(data, CancellationToken.None);

		public async Task Merge(Adjustment data, CancellationToken token)
		{
			TableOperation mergeOperation = TableOperation.InsertOrMerge(data);
			await table.ExecuteAsync(mergeOperation, TableManager.TableOptions, null, token);
		}

		public async Task MergeBatch(IEnumerable<Adjustment> data, CancellationToken token)
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

		public async Task<List<Adjustment>> GetAll(CancellationToken token)
		{
			TableQuery<Adjustment> query = new TableQuery<Adjustment>();
			List<Adjustment> result = new List<Adjustment>();
			TableContinuationToken continuationToken = null;
			do
			{
				TableQuerySegment<Adjustment> tableQueryResult =
					await table.ExecuteQuerySegmentedAsync(query, continuationToken, TableManager.TableOptions, null, token);

				result.AddRange(tableQueryResult.Results);
			} while (continuationToken != null);

			return result;
		}
	}
}