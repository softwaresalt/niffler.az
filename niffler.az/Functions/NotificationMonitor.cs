using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Niffler.Data;
using Niffler.Modules;
using System;
using System.Threading.Tasks;

namespace Niffler
{
	/// <summary>
	/// Responsible for monitoring notification queue;
	/// sends email notifications based on notification type.
	/// </summary>
	public static class NotificationMonitor
	{
		[FunctionName("NotificationMonitor")]
		public static async Task RunAsync([QueueTrigger("notification")] string message, ILogger log)
		{
			string formatter = "Task: {task}#Error Message: {error}#Stack Trace: {trace}".Replace("#", Environment.NewLine);
			try
			{
				NotificationDTO dto = JsonConvert.DeserializeObject<NotificationDTO>(message);
				using (Mail mail = new Mail("smtp.gmail.com", 587))
				{
					mail.SetFromAddress(dto.FromAddress, "Niffler.Trader");
					mail.SetCredentials(Cache.NotificationCredentialUN, Cache.NotificationCredentialPW);
					mail.Subject = $"Niffler.Trader.Alert: {dto.AccountName}: {dto.AlertType}";
					switch (dto.AlertType)
					{
						case AlertType.Position:
							mail.Body = $"Ticker:{dto.Symbol};{dto.PositionType} {dto.Quantity} Shares@{dto.FillPrice}:Filled@{dto.FillDate};P/L:{dto.PL}";
							break;

						case AlertType.DailyPerformance:
							mail.Body = $"Total Real P/L($): {dto.PL}; Ratio P/L(%): {dto.PLPct}";
							break;
					}
					SendStatus status = await mail.SendToAsync(dto.ToAddress.Split(';'));
					if (status == SendStatus.InvalidConfig) { log.LogWarning("Attempted email does not contain all valid parameters."); }
					if (status == SendStatus.Error) { throw mail.Error; }
				};
				log.LogInformation($"Trade Service Notification Message: {message}");
			}
			catch (Exception e)
			{
				using (log.BeginScope("Notifications for Date: {Date}", DateTime.Now.Date))
				{
					log.LogError(e, formatter, message, e.Message, e.StackTrace);
				}
				throw;
			}
		}
	}
}