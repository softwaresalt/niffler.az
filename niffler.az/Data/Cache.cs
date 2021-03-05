using Alpaca.Markets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Storage.Queue;
using Niffler.Data.APCA;
using Niffler.Data.ATS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using s = Microsoft.Azure.Storage;

namespace Niffler.Data
{
	/// <summary>
	/// In-app memory cache of commonly used data to speed up some operations
	/// by reducing number of table storage lookup calls.
	/// App is not designed for multi-tenant use.
	/// </summary>
	public class Cache
	{
		private static string storageConnection;
		private static string tradeOperationQueueName;
		private static string tradeDispatchMonitorQueueName;
		private static string notificationQueueName;
		private static string apcaAPIKeyID;
		private static string apcaAPISecretKey;
		private static string notificationCredUN;
		private static string notificationCredPW;
		private static string notifyFromAddress;
		private static bool useKeyvault;

		//private static TradingEnvironment tradeEnvironment;
		private static IEnvironment environment;

		private static s.CloudStorageAccount storageAccount;
		private static CloudStorageAccount tableStorageAccount;
		private static CloudQueueClient queueClient;

		//private static CloudTableClient tableClient;
		private static IAlpacaDataClient dataClientAPCA;

		public const int IDCacheQueueLimit = 100;

		public static void SetAPCAKeys(string apiKeyID, string apiSecretKey)
		{
			apcaAPIKeyID = apiKeyID;
			apcaAPISecretKey = apiSecretKey;
		}

		public static void SetEmailCredentials(string un, string pw, string fromAddress)
		{
			notificationCredUN = un;
			notificationCredPW = pw;
			notifyFromAddress = fromAddress;
		}

		public static void SetQueueNames(string operations, string monitor)
		{
			tradeOperationQueueName = operations;
			tradeDispatchMonitorQueueName = monitor;
		}

		public static string NotificationCredentialUN
		{
			get
			{
				if (notificationCredUN == null) { notificationCredUN = Environment.GetEnvironmentVariable("Notification-Credential-UN"); }
				return notificationCredUN;
			}
		}

		public static string NotificationCredentialPW
		{
			get
			{
				if (notificationCredPW == null) { notificationCredPW = Environment.GetEnvironmentVariable("Notification-Credential-PW"); }
				return notificationCredPW;
			}
		}

		public static string NotifyFromAddress
		{
			get
			{
				if (notifyFromAddress == null) { notifyFromAddress = Environment.GetEnvironmentVariable("NotifyFromAddress"); }
				return notifyFromAddress;
			}
		}

		public static string APCAAPIKeyID
		{
			get
			{
				if (apcaAPIKeyID == null) { apcaAPIKeyID = Environment.GetEnvironmentVariable("APCA-API-KEY-ID"); }
				return apcaAPIKeyID;
			}
		}

		public static string APCAAPISecretKey
		{
			get
			{
				if (apcaAPISecretKey == null) { apcaAPISecretKey = Environment.GetEnvironmentVariable("APCA-API-SECRET-KEY"); }
				return apcaAPISecretKey;
			}
		}

		public static bool UseKeyvault
		{
			get
			{
				return bool.TryParse(Environment.GetEnvironmentVariable("USE_KEYVAULT"), out useKeyvault) ? useKeyvault : false;
			}
		}

		public static string TradeOperationQueueName
		{
			get
			{
				if (tradeOperationQueueName == null) { tradeOperationQueueName = Environment.GetEnvironmentVariable("TradeOperationQueueName"); }
				return tradeOperationQueueName;
			}
		}

		public static string TradeDispatchMonitorQueueName
		{
			get
			{
				if (tradeDispatchMonitorQueueName == null) { tradeDispatchMonitorQueueName = Environment.GetEnvironmentVariable("TradeDispatchMonitorQueueName"); }
				return tradeDispatchMonitorQueueName;
			}
		}

		public static string NotificationMonitorQueueName
		{
			get
			{
				if (notificationQueueName == null) { notificationQueueName = Environment.GetEnvironmentVariable("NotificationMonitorQueueName"); }
				return notificationQueueName;
			}
		}

		public static string StorageConnection
		{
			get
			{
				if (storageConnection == null) { storageConnection = Environment.GetEnvironmentVariable("AzureWebJobsStorage"); }
				return storageConnection;
			}
			set { storageConnection = value; }
		}

		public static s.CloudStorageAccount StorageAccount
		{
			get
			{
				if (storageAccount == null) { storageAccount = s.CloudStorageAccount.Parse(StorageConnection); }
				return storageAccount;
			}
		}

		public static CloudStorageAccount TableStorageAccount
		{
			get
			{
				if (tableStorageAccount == null) { tableStorageAccount = CloudStorageAccount.Parse(StorageConnection); }
				return tableStorageAccount;
			}
		}

		public static CloudQueueClient QueueClient
		{
			get
			{
				if (queueClient == null) { queueClient = StorageAccount.CreateCloudQueueClient(); }
				return queueClient;
			}
		}

		//public static CloudTableClient TableClient
		//{
		//	get
		//	{
		//		if (tableClient == null) { tableClient = tableStorageAccount.CreateCloudTableClient(); }
		//		return tableClient;
		//	}
		//}

		public static IAlpacaDataClient APCADataClient
		{
			get
			{
				if (dataClientAPCA == null) { dataClientAPCA = Environments.Live.GetAlpacaDataClient(new SecretKey(APCAAPIKeyID, APCAAPISecretKey)); }
				return dataClientAPCA;
			}
		}

		public static IAlpacaTradingClient TradingClient(long accountID)
		{
			SecretKey secretKey;
			if (UseKeyvault)
			{
				var secrets = KeyVaultSecrets[accountID];
				secretKey = new SecretKey(secrets[SecretType.AlpacaAPIKeyID], secrets[SecretType.AlpacaAPISecretKey]);
			}
			else
			{
				secretKey = new SecretKey(APCAAPIKeyID, APCAAPISecretKey);
			}
			return environment.GetAlpacaTradingClient(secretKey);
		}

		public static IAlpacaTradingClient TradingClient()
		{
			return environment.GetAlpacaTradingClient(new SecretKey(APCAAPIKeyID, APCAAPISecretKey));
		}

		public static Clock MarketHours { get; private set; }

		public static Dictionary<Boundary, TimeSpan> TimeBoundary { get; } = new Dictionary<Boundary, TimeSpan>(12)
			{
				{ Boundary.T0930, new TimeSpan(09, 30, 00) },{ Boundary.T0945, new TimeSpan(09, 45, 00) },{ Boundary.T1000, new TimeSpan(10, 00, 00) },{ Boundary.T1030, new TimeSpan(10, 30, 00) },
				{ Boundary.T1100, new TimeSpan(11, 00, 00) },{ Boundary.T1240, new TimeSpan(12, 40, 00) },{ Boundary.T1330, new TimeSpan(13, 30, 00) },{ Boundary.T1400, new TimeSpan(14, 00, 00) },
				{ Boundary.T1500, new TimeSpan(15, 00, 00) },{ Boundary.T1530, new TimeSpan(15, 30, 00) },{ Boundary.T1550, new TimeSpan(15, 50, 00) },{ Boundary.T1600, new TimeSpan(16, 00, 00) }
			};

		public static bool IsInitialized { get; set; } = false;
		public static Dictionary<string, bool> TickerInProcess { get; set; } = new Dictionary<string, bool>();
		public static Dictionary<long, bool> OperationInProcess { get; set; } = new Dictionary<long, bool>();
		public static SemaphoreSlim AccountUpdateSemaphore { get; set; } = new SemaphoreSlim(1, 1);
		public static SemaphoreSlim CacheInitSemaphore { get; set; } = new SemaphoreSlim(1, 1);
		public static SemaphoreSlim IDSemaphore { get; set; } = new SemaphoreSlim(1, 1);
		public static Dictionary<long, Dictionary<OperationType, SemaphoreSlim>> AccountOperationSemaphores { get; set; } = new Dictionary<long, Dictionary<OperationType, SemaphoreSlim>>();
		public static Dictionary<long, SemaphoreSlim> AccountSemaphores { get; set; } = new Dictionary<long, SemaphoreSlim>();
		public static List<DispatchSymbol> DispatchSymbols { get; set; } = new List<DispatchSymbol>();
		public static List<Account> Accounts { get; set; } = new List<Account>();

		//public static List<Secret> Secrets { get; set; } = new List<Secret>();
		public static Dictionary<long, Dictionary<SecretType, string>> KeyVaultSecrets { get; set; } = new Dictionary<long, Dictionary<SecretType, string>>();

		public static List<MarketSchedule> MarketSchedules { get; set; } = new List<MarketSchedule>();
		public static List<Workflow> Workflows { get; set; } = new List<Workflow>();
		public static List<WorkflowOperation> WorkflowOperations { get; set; } = new List<WorkflowOperation>();
		public static List<Service> Services { get; set; } = new List<Service>();
		public static List<ServiceParameter> ServiceParameters { get; set; } = new List<ServiceParameter>();
		public static List<ServiceLoginParameter> ServiceLoginParameters { get; set; } = new List<ServiceLoginParameter>();
		public static List<ServiceQueue> ServiceQueues { get; set; } = new List<ServiceQueue>();
		public static List<ScreenLog> ScreenLogs { get; set; } = new List<ScreenLog>();
		public static List<QuoteLog> QuoteLogs { get; set; } = new List<QuoteLog>();
		public static List<TradeBook> TradeBooks { get; set; } = new List<TradeBook>();
		public static TimeZoneInfo EST { get; set; } = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
		public static Queue<long> IDCache { get; set; }

		public static async Task InitializeAsync()
		{
			TradingEnvironment tradeEnvironment;
			List<Secret> secrets = new List<Secret>();
			if (!Enum.TryParse<TradingEnvironment>(Environment.GetEnvironmentVariable("TradingEnvironment"), out tradeEnvironment))
			{
				tradeEnvironment = TradingEnvironment.Paper;
			}
			switch (tradeEnvironment)
			{
				case TradingEnvironment.Paper:
					environment = Environments.Paper;
					break;

				case TradingEnvironment.Live:
					environment = Environments.Live;
					break;
			}
			if (Accounts.Count == 0)
			{
				using (var ops = new AccountOps())
				{
					var itemList = await ops.GetAll();
					if (itemList != null && itemList.Count > 0) { Accounts.AddRange(itemList); }
				}
			}
			using (var ops = new SecretOps())
			{
				var itemList = await ops.GetAll();
				if (itemList != null && itemList.Count > 0) { secrets.AddRange(itemList); }
			}
			if (MarketSchedules.Count == 0)
			{
				using (var ops = new MarketScheduleOps())
				{
					var itemList = await ops.GetAllByKey(Util.CurrentDateESTString);
					if (itemList != null && itemList.Count > 0) { MarketSchedules.AddRange(itemList); }
				}
			}

			using (var ops = new WorkflowOps())
			{
				foreach (Account item in Accounts)
				{
					var itemList = await ops.GetAllEnabledByKey(item.PartitionKey, CancellationToken.None);
					if (itemList != null && itemList.Count > 0) { Workflows.AddRange(itemList); }
				}
			}
			using (var ops = new WorkflowOperationOps())
			{
				var itemList = await ops.GetAllEnabled();
				if (itemList != null && itemList.Count > 0)
				{
					foreach (Workflow item in Workflows)
					{
						WorkflowOperations.AddRange(itemList.FindAll(x => x.WorkflowID == item.WorkflowID));
					}
				}
			}
			using (var ops = new QuoteLogOps())
			{
				foreach (Workflow item in Workflows)
				{
					var itemList = await ops.GetAllByKey(item.WorkflowID, Util.CurrentDateEST, CancellationToken.None);
					if (itemList != null && itemList.Count > 0) { QuoteLogs.AddRange(itemList); }
				}
			}
			if (Services.Count == 0)
			{
				using (var ops = new ServiceOps())
				{
					var itemList = await ops.GetAllEnabled(CancellationToken.None);
					if (itemList != null && itemList.Count > 0) { Services.AddRange(itemList); }
				}
			}
			using (var ops = new ServiceParameterOps())
			{
				foreach (Service item in Services)
				{
					var itemList = await ops.GetAllEnabledByKey(item.PartitionKey, CancellationToken.None);
					if (itemList != null && itemList.Count > 0) { ServiceParameters.AddRange(itemList); }
				}
			}
			using (var ops = new ServiceLoginParameterOps())
			{
				foreach (Service item in Services)
				{
					var itemList = await ops.GetAllByKey(item.PartitionKey, CancellationToken.None);
					if (itemList != null && itemList.Count > 0) { ServiceLoginParameters.AddRange(itemList); }
				}
			}
			if (ServiceQueues.Count == 0)
			{
				using (var ops = new ServiceQueueOps())
				{
					var itemList = await ops.GetAllEnabledByKey(Util.CurrentDateESTString);
					if (itemList != null && itemList.Count > 0) { ServiceQueues.AddRange(itemList); }
				}
			}
			if (DispatchSymbols.Count == 0)
			{
				using (var ops = new DispatchSymbolOps())
				{
					var itemList = await ops.GetAllByTradeDate(Util.CurrentDateEST);
					if (itemList != null && itemList.Count > 0) { DispatchSymbols.AddRange(itemList); }
				}
			}
			if (MarketHours == null)
			{
				IClock clock = await environment.GetAlpacaTradingClient(new SecretKey(APCAAPIKeyID, APCAAPISecretKey)).GetClockAsync();
				MarketHours = new Clock(clock, EST);
			}
			Dictionary<OperationType, SemaphoreSlim> operations = new Dictionary<OperationType, SemaphoreSlim>();
			operations.Add(OperationType.MarketBuy, new SemaphoreSlim(1, 1));
			operations.Add(OperationType.MarketSell, new SemaphoreSlim(1, 1));
			foreach (Account account in Accounts)
			{
				if (!AccountSemaphores.ContainsKey(account.AccountID)) { AccountSemaphores.Add(account.AccountID, new SemaphoreSlim(1, 1)); }
				if (!AccountOperationSemaphores.ContainsKey(account.AccountID)) { AccountOperationSemaphores.Add(account.AccountID, operations); }
			}
			if (UseKeyvault) { getKeyVaultSecrets(secrets); }
			IDCache = new Queue<long>(KeyGen.GenIDList(IDCacheQueueLimit));
			IsInitialized = true;
		}

		private static void getKeyVaultSecrets(List<Secret> secrets)
		{
			string keyVaultName = Environment.GetEnvironmentVariable("KEY_VAULT_NAME");
			string uri = $"https://{keyVaultName}.vault.azure.net";
			SecretClient client = new SecretClient(new Uri(uri), new DefaultAzureCredential());
			foreach (Secret secret in secrets.OrderBy(x => x.AccountID))
			{
				string keyLookup = Util.ConformedKey(secret.PartitionKey, secret.RowKey, '-');
				checkAddSecret(client, secret.AccountID, secret.eSecretType, secret.KeyName, keyLookup);
			}
		}

		private static void checkAddSecret(SecretClient client, long accountID, SecretType type, string keyName, string keyLookup)
		{
			Dictionary<SecretType, string> lclAcctVault;
			KeyVaultSecret kvSecret = client.GetSecret(keyLookup);
			if (KeyVaultSecrets.ContainsKey(accountID)) { lclAcctVault = KeyVaultSecrets[accountID]; } else { lclAcctVault = new Dictionary<SecretType, string>(); };
			lclAcctVault.Add(type, kvSecret.Value);
			if (KeyVaultSecrets.ContainsKey(accountID))
			{
				KeyVaultSecrets[accountID] = lclAcctVault;
			}
			else
			{
				KeyVaultSecrets.Add(accountID, lclAcctVault);
			}
		}

		public static async Task LoadScreenData(long workflowID, DateTime queueDate)
		{
			if (ScreenLogs.Count == 0)
			{
				var ops = new ScreenLogOps();
				var itemList = await ops.GetAllByKey(workflowID, queueDate);
				if (itemList != null && itemList.Count > 0) { ScreenLogs.AddRange(itemList); }
			}
		}

		public static async Task LoadTradeBookData(long workflowID, DateTime queueDate)
		{
			if (TradeBooks.Count == 0 || !TradeBooks.Exists(x => x.PartitionKey == Util.ConformedKey(workflowID.ToString(), queueDate)))
			{
				var ops = new TradeBookOps();
				var itemList = await ops.GetAllByKey(workflowID, queueDate);
				if (itemList != null && itemList.Count > 0) { TradeBooks.AddRange(itemList); }
			}
		}

		public static void MergeTradeBook(List<TradeBook> trades)
		{
			foreach (TradeBook trade in trades)
			{
				MergeTradeIntoBook(trade);
			}
		}

		public static void MergeTradeIntoBook(TradeBook trade)
		{
			if (!TradeBooks.Exists(x => x.OrderID == trade.OrderID)) { TradeBooks.Add(trade); }
			if (TradeBooks.Exists(x => x.OrderID == trade.OrderID && x.Amount == 0))
			{
				TradeBooks.RemoveAll(x => x.OrderID == trade.OrderID);
				TradeBooks.Add(trade);
			}
		}
	}
}