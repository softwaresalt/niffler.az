using Alpaca.Markets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Logging;
using System.Reflection;

//using Niffler.Notification;
using Niffler.Data;
using Niffler.Data.APCA;
using Niffler.Data.ATS;
using Niffler.Modules;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Niffler.Test
{
	public class Tests
	{
		private string emailTo = String.Empty;
		private string keyVault = String.Empty;
		private string clientID = String.Empty;

		//private string clientSecret = String.Empty;
		//private string tentantID = String.Empty;
		private string apcaAcctID = String.Empty;

		[SetUp]
		public void Setup()
		{
			Assembly me = Assembly.GetExecutingAssembly();
			Configuration config = ConfigurationManager.OpenExeConfiguration(me.ManifestModule.Assembly.Location);
			string apiKeyID = config.AppSettings.Settings["APCA-API-KEY-ID"].Value;
			string apiSecretKey = config.AppSettings.Settings["APCA-API-SECRET-KEY"].Value;
			string opQueue = config.AppSettings.Settings["TradeOperationQueueName"].Value;
			string monitorQueue = config.AppSettings.Settings["TradeDispatchMonitorQueueName"].Value;
			string emailFrom = config.AppSettings.Settings["NotifyFromAddress"].Value;
			string emailCredUN = config.AppSettings.Settings["Notification-Credential-UN"].Value;
			string emailCredPW = config.AppSettings.Settings["Notification-Credential-PW"].Value;
			emailTo = config.AppSettings.Settings["NotifyToAddresses"].Value;
			apcaAcctID = config.AppSettings.Settings["APCA_AccountID"].Value;

			keyVault = config.AppSettings.Settings["KEY_VAULT_NAME"].Value;
			clientID = config.AppSettings.Settings["AZURE_CLIENT_ID"].Value;
			//clientSecret = config.AppSettings.Settings["AZURE_CLIENT_SECRET"].Value;
			//tentantID = config.AppSettings.Settings["AZURE_TENANT_ID"].Value;

			Cache.StorageConnection = config.AppSettings.Settings["AzureWebJobsStorage"].Value;
			Cache.SetAPCAKeys(apiKeyID, apiSecretKey);
			Cache.SetQueueNames(opQueue, monitorQueue);
			Cache.SetEmailCredentials(emailCredUN, emailCredPW, emailFrom);
		}

		[Test]
		public void TestGetClock()
		{
			int tradeableCount = Util.CalculatePositionCount(120000M);
			IAlpacaTradingClient client = Environments.Paper.GetAlpacaTradingClient(new SecretKey(Cache.APCAAPIKeyID, Cache.APCAAPISecretKey));
			Clock clock = Cache.MarketHours;
		}

		[Test]
		public async Task TestGetQuoteData()
		{
			string symbol = "ZM";
			long workflowID = 493853015594237998, serviceQueueID = 675347543856513024;
			DateTime queueDate = DateTime.Now;
			IReadOnlyList<IAgg> quotes = await Util.GetRealtimeQuoteData(symbol);
			List<QuoteLog> ql = (
				from quote in quotes
				select new QuoteLog(Util.ConformedKey(workflowID.ToString(), queueDate))
				{
					Symbol = symbol,
					WorkflowID = workflowID,
					PriceOpen = (double)quote.Open,
					PriceHigh = (double)quote.High,
					PriceLow = (double)quote.Low,
					PriceClose = (double)quote.Close,
					ServiceQueueID = serviceQueueID,
					Volume = quote.Volume,
					TimeStampRecorded = TimeZoneInfo.ConvertTimeFromUtc(quote.TimeUtc.Value, Cache.EST)
				}
				).ToList();
			ql.ForEach(x => x.SetRowKey());
			Console.WriteLine(ql.First().TimeStampRecorded.ToString());
		}

		[Test]
		public async Task TestPostOrder()
		{
			//IAccount account = await Cache.TradingClient.GetAccountAsync();
			//decimal lastEquity = account.LastEquity;
			//ListOrdersRequest lor = new ListOrdersRequest() { OrderStatusFilter = OrderStatusFilter.Open, OrderListSorting = SortDirection.Ascending };
			//IReadOnlyList<IPosition> positions = await Cache.TradingClient.ListPositionsAsync();
			//IReadOnlyList<IOrder> orders = await Cache.TradingClient.ListOrdersAsync(lor);
			//IAsset asset = await Cache.TradingClient.GetAssetAsync("BLKB");
			//IPosition position = await Cache.TradingClient.GetPositionAsync("BLKB");
			try
			{
				NewOrderRequest nor = new NewOrderRequest("BLKB", 0, OrderSide.Buy, OrderType.Market, TimeInForce.Day);
				//IOrder order = await Cache.TradingClient.PostOrderAsync(nor);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}
		}

		[Test]
		public async Task TestGetAsset()
		{
			string symbol = "HEI-A"; IAsset asset;
			try
			{
				asset = await Cache.TradingClient().GetAssetAsync(symbol);
			}
			catch (RestClientErrorException e)
			{
				throw e;
			}
		}

		public async Task InitiliazeCache()
		{
			if (!Cache.IsInitialized)
			{
				await Cache.CacheInitSemaphore.WaitAsync();
				try
				{
					if (!Cache.IsInitialized)
					{
						await Cache.InitializeAsync();
					}
				}
				finally
				{
					Cache.CacheInitSemaphore.Release();
				}
			}
		}

		[Test]
		public async Task OperationDispatcherTest()
		{
			await InitiliazeCache();
			try
			{
				DateTime date = Util.CurrentDateEST;
				TimeSpan zeroSpan = TimeSpan.Zero;
				List<ServiceQueue> queues = new List<ServiceQueue>();
				foreach (Workflow workflow in Cache.Workflows)
				{
					long id = await KeyGen.NewID();
					ServiceQueueDTO.TaskDTO task = new ServiceQueueDTO.TaskDTO(id, workflow, Cache.WorkflowOperations.FindAll(x => x.WorkflowID == workflow.WorkflowID));
					JobDTO job = new JobDTO(task);
					if (
						!Cache.ServiceQueues.Exists(x => x.QueueDate.Date == task.QueueDate.Date && x.WorkflowID == task.WorkflowID && x.CompletionDate == null) && //AND: No queue entries for current date
						Cache.WorkflowOperations.Exists(x => x.WorkflowID == task.WorkflowID)
					)
					{
						var item = new ServiceQueue(task);
						Cache.ServiceQueues.Add(item);
						queues.Add(item);
						DateTime startTime = task.QueueDate.Add(task.StartTimeEST);
						TimeSpan delay = (startTime - Util.CurrentDateTimeEST) < zeroSpan ? zeroSpan : (startTime - Util.CurrentDateTimeEST);
						await Store.PushMessageToQueue(Cache.TradeOperationQueueName, Store.Serialize(job), delay);
					}
				}
				using (var ops = new ServiceQueueOps())
				{
					if (queues.Count > 0) { await ops.MergeBatch(queues); }
				}
			}
			catch (Exception e)
			{
				throw;
			}
		}

		[Test]
		public async Task NifflerMarketBuyTest()
		{
			await InitiliazeCache();
			ILogger logger = null;
			bool isWorkDone = false;
			try
			{
				TradeDispatchDTO dto = new TradeDispatchDTO()
				{
					AccountID = 493853015594238003,
					ServiceQueueID = 698307516072722432,
					WorkflowID = 493853015594238005,
					ModuleType = ModuleType.Niffler,
					TradeBalance = 8225.551,
					Symbol = "PCTY",
					TradeDate = DateTime.Parse("2020-04-10T00:00:00Z"),
					EndPreviousSessionUTC = DateTime.Parse("2020-04-08T18:00:00Z"),
					StartCurrentSessionUTC = DateTime.SpecifyKind(DateTime.Parse("2020-04-09T13:30:00Z"), DateTimeKind.Utc),
					EndCurrentSessionUTC = DateTime.Parse("2020-04-09T20:00:00Z"),
					IsShortable = true
				};
				Trade trade = new Trade()
				{
					Symbol = dto.Symbol,
					State = TradeState.Open,
					TradeRuleGroup = TradeRuleGroup.Group2,
					TradeRuleCase = BuySellCase.RGBuyCase1,
					Quantity = 100,
					LastQuotePrice = 101,
					LastQuoteVolume = 1374,
					CurrentVolume = 50000,
					TradeDT = dto.TradeDate
				};
				NifflerMod module = new NifflerMod(new ServiceManager(logger));
				isWorkDone = await module.RunAsync(dto);
				//isWorkDone = await module.marketBuy(trade);
			}
			catch (Exception e)
			{
				throw;
			}
		}

		[Test]
		public async Task SendGmailTest()
		{
			try
			{
				using (SmtpClient client = new SmtpClient("smtp.gmail.com", 587))
				{
					client.EnableSsl = true;
					client.Credentials = new NetworkCredential(Cache.NotificationCredentialUN, Cache.NotificationCredentialPW);
					client.DeliveryMethod = SmtpDeliveryMethod.Network;
					client.DeliveryFormat = SmtpDeliveryFormat.SevenBit;
					MailAddress addressTo = new MailAddress(emailTo);
					MailAddress addressFrom = new MailAddress(Cache.NotifyFromAddress, "Niffler.Trader", Encoding.ASCII);
					MailMessage message = new MailMessage(addressFrom, addressTo)
					{
						Subject = "MSFT:Open@36.50;Shares@50"
					};
					await client.SendMailAsync(message);
				}
			}
			catch (Exception e)
			{
				throw e;
			}
		}

		[Test]
		public async Task SendNotificationMailTest()
		{
			using (Mail mail = new Mail("smtp.gmail.com", 587))
			{
				mail.SetFromAddress(Cache.NotifyFromAddress, "Niffler.Trader");
				mail.SetCredentials(Cache.NotificationCredentialUN, Cache.NotificationCredentialPW);
				mail.Subject = "Niffler.Trader.Alert";
				mail.Body = $"JACK:Closed@50.07;Shares:343;Filled@2020-04-16T10:26:16;P/L:13.7200";
				SendStatus status = await mail.SendToAsync(emailTo.Split(';'));
			};
		}

		[Test]
		public async Task TestGetSecret()
		{
			await InitiliazeCache();
			try
			{
				Uri uri = new Uri($"https://{keyVault}.vault.azure.net");
				TokenCredentialOptions options = new TokenCredentialOptions();
				ManagedIdentityCredential credential = new ManagedIdentityCredential(clientID);
				SecretClient client = new SecretClient(uri, credential);
				string keyLookup = Util.ConformedKey(apcaAcctID, "AlpacaAPIKeyID", '-');
				KeyVaultSecret kvSecret = client.GetSecret(keyLookup);
			}
			catch (Exception e)
			{
				throw e;
			}
		}
	}
}