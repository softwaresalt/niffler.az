using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Niffler.Data;
using Niffler.Data.ATS;
using Niffler.Interfaces;
using Niffler.Modules;
using System;
using System.Threading.Tasks;

namespace Niffler
{
	/// <summary>
	/// TradeDispatchMonitor: serverless function to put dispatched trade workflows into process.
	/// </summary>
	public static class TradeDispatchMonitor
	{
		[FunctionName("TradeDispatchMonitor")]
		public static async Task RunAsync([QueueTrigger("tradedispatchmonitor")] string message, ILogger log)
		{
			string formatter = "Task: {task}#Error Message: {error}#Stack Trace: {trace}".Replace("#", Environment.NewLine);
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
			TradeDispatchDTO job = new TradeDispatchDTO();
			bool isProcessEngaged = false;
			try
			{
				job = Store.Deserialize<TradeDispatchDTO>(message);
				//Ensure only one instance of job is put into process.
				if (!Cache.TickerInProcess.ContainsKey(job.Symbol) && Cache.TickerInProcess.TryAdd(job.Symbol, true)) { isProcessEngaged = true; }
				if (Cache.TickerInProcess.ContainsKey(job.Symbol) && !Cache.TickerInProcess[job.Symbol]) { Cache.TickerInProcess[job.Symbol] = true; isProcessEngaged = true; }
				if (isProcessEngaged)
				{
					IWorkflowManager wf = new TradeDispatchManager(log);
					await wf.RunAsync(job);
					log.LogInformation($"Trade Service Processed Message: {message}");
				}
			}
			catch (Exception e)
			{
				using (log.BeginScope("ServiceQueueID: {ID}", job.ServiceQueueID))
				{
					log.LogError(e, formatter, message, e.Message, e.StackTrace);
				}
				throw;
			}
			finally
			{
				if (isProcessEngaged && Cache.TickerInProcess.ContainsKey(job.Symbol)) { Cache.TickerInProcess[job.Symbol] = false; }
			}
		}
	}

	public static class TradeDispatchMonitorPoison
	{
		[FunctionName("TradeDispatchMonitorPoison")]
		public static async Task RunAsync([QueueTrigger("tradedispatchmonitor-poison")] string message, ILogger log)
		{
			string formatter = "Task: {task}#Error Message: {error}#Stack Trace: {trace}".Replace("#", Environment.NewLine);
			TradeDispatchDTO job = new TradeDispatchDTO();
			try
			{
				job = Store.Deserialize<TradeDispatchDTO>(message);
				await Util.RetryOnExceptionAsync(async () => await Store.PushMessageToQueue(Cache.TradeDispatchMonitorQueueName, Store.Serialize(job), TimeSpan.FromMinutes(1)), log);
				Account account = Cache.Accounts.Find(x => x.AccountID == job.AccountID);
				using (Mail mail = new Mail("smtp.gmail.com", 587))
				{
					mail.SetFromAddress(Cache.NotifyFromAddress, "Niffler.Trader");
					mail.SetCredentials(Cache.NotificationCredentialUN, Cache.NotificationCredentialPW);
					mail.Subject = $"Niffler.Trader.Alert.Error: {account.Description}";
					mail.Body = Store.Serialize(job);
					SendStatus status = await mail.SendToAsync(account.NotifyRecipient.Split(';'));
					if (status == SendStatus.InvalidConfig) { log.LogWarning("Attempted email does not contain all valid parameters."); }
					if (status == SendStatus.Error) { throw mail.Error; }
				};
			}
			catch (Exception e)
			{
				using (log.BeginScope("ServiceQueueID: {ID}", job.ServiceQueueID))
				{
					log.LogError(e, formatter, message, e.Message, e.StackTrace);
				}
				throw;
			}
		}
	}
}