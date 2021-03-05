using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Storage.Queue;
using System;
using System.Net;
using System.Threading.Tasks;
using rp = Microsoft.Azure.Storage.RetryPolicies;

namespace Niffler.Data.ATS
{
	public static class TableManager
	{
		private static CloudTableClient _tableClient;

		private static readonly object Gate = new object();

		private static bool _storageInitialized;

		static TableManager()
		{
			InitializeStorage();
		}

		public static QueueRequestOptions QueueOptions { get; private set; } = new QueueRequestOptions()
		{ RetryPolicy = new rp.LinearRetry(TimeSpan.FromSeconds(5), 10), MaximumExecutionTime = TimeSpan.FromSeconds(60) };

		public static TableRequestOptions TableOptions { get; private set; } = new TableRequestOptions()
		{ RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(5), 10), MaximumExecutionTime = TimeSpan.FromSeconds(60) };

		public static CloudTable GetCloudTable(Type type)
		{
			return GetStorageTable(type.Name).GetAwaiter().GetResult();
		}

		private static async Task<CloudTable> GetStorageTable(string tableName)
		{
			var table = _tableClient.GetTableReference(tableName);
			await table.CreateIfNotExistsAsync();

			return table;
		}

		private static void InitializeStorage()
		{
			if (_storageInitialized)
			{
				return;
			}

			lock (Gate)
			{
				if (_storageInitialized)
				{
					return;
				}

				try
				{
					CloudStorageAccount storageAccount = Cache.TableStorageAccount;
					_tableClient = storageAccount.CreateCloudTableClient();

					_tableClient.DefaultRequestOptions = new TableRequestOptions
					{
						RetryPolicy = new ExponentialRetry(TimeSpan.FromMilliseconds(500), 5)
					};
				}
				catch (WebException)
				{
					throw new WebException("Storage services initialization failure. "
							+ "Check your storage account configuration settings. If running locally, "
							+ "ensure that the Development Storage service is running.");
				}

				_storageInitialized = true;
			}
		}

		//public static IList<ITableEntity> GetServiceTypeEntity(Enums.ServiceType svcType)
		//{
		//	IList<ITableEntity> entityList = (IList<ITableEntity>)new List<ScreenLog>();
		//	switch (svcType)
		//	{
		//		case Enums.ServiceType.StockScreener:
		//			entityList = (IList<ITableEntity>)new List<ScreenLog>();
		//			break;
		//		case Enums.ServiceType.StockQuote:
		//		case Enums.ServiceType.BatchStockQuoteRealtime:
		//			entityList = (IList<ITableEntity>)new List<QuoteLog>();
		//			break;
		//		case Enums.ServiceType.MarketOrderService:
		//		case Enums.ServiceType.BatchMarketOrderService:
		//			entityList = (IList<ITableEntity>)new List<TradeBook>();
		//			break;
		//	}
		//	return entityList;
		//}
	}
}