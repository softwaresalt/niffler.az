using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Niffler.Data;
using Niffler.Data.ATS;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Niffler
{
	/// <summary>
	/// Designed to kick-off at 9:20 EST every weekday.
	/// Determines if current day is a valid trading day on calendar.
	/// Dispatcher/Agent queues all workflows configured in table storage.
	/// </summary>
	public static class TradeOperationDispatcher
	{
		//Run at 09:30 AM, MON through FRI
		[FunctionName("TradeOperationDispatcher")]
#if DEBUG
		public static async Task RunAsync([TimerTrigger("0 20 9 * * MON-FRI", RunOnStartup = false)] TimerInfo opGenTimer, ILogger log) //Test trigger method definition
#else
		public static async Task RunAsync([TimerTrigger("0 20 9 * * MON-FRI", RunOnStartup = false)]TimerInfo opGenTimer, ILogger log)
#endif
		{
			string formatter = "Error Message: {error}#Stack Trace: {trace}".Replace("#", Environment.NewLine);
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
			try
			{
				//ServiceQueueDTO queueDTO = new ServiceQueueDTO();
				DateTime date = Util.CurrentDateEST;
				List<ServiceQueue> queues = new List<ServiceQueue>();
				foreach (Workflow workflow in Cache.Workflows)
				{
					long id = await KeyGen.NewID();
					ServiceQueueDTO.TaskDTO task = new ServiceQueueDTO.TaskDTO(id, workflow, Cache.WorkflowOperations.FindAll(x => x.WorkflowID == workflow.WorkflowID));
					JobDTO job = new JobDTO(task);
					//DayOfWeek day = task.QueueDate.DayOfWeek;
					if (
						Cache.MarketHours.NextOpen.Date == date && Cache.MarketHours.NextClose.TimeOfDay == Cache.TimeBoundary[Boundary.T1600] &&
						//!(day == DayOfWeek.Saturday || day == DayOfWeek.Sunday) && //IF: Not a weekend
						//!Cache.MarketSchedules.Exists(x => x.ScheduleDate == task.QueueDate.Date) && //AND: NOT Scheduled Market Holiday (or reduced period day)
						!Cache.ServiceQueues.Exists(x => x.QueueDate.Date == task.QueueDate.Date && x.WorkflowID == task.WorkflowID && x.CompletionDate == null) && //AND: No queue entries for current date
						Cache.WorkflowOperations.Exists(x => x.WorkflowID == task.WorkflowID)
					)
					{
						var item = new ServiceQueue(task);
						Cache.ServiceQueues.Add(item);
						queues.Add(item);
						DateTime startTime = task.QueueDate.Add(task.StartTimeEST);
						TimeSpan delay = (startTime - Util.CurrentDateTimeEST) < TimeSpan.Zero ? TimeSpan.Zero : (startTime - Util.CurrentDateTimeEST);
						await Store.PushMessageToQueue(Cache.TradeOperationQueueName, Store.Serialize(job), delay);
					}
				}
				using (var ops = new ServiceQueueOps())
				{
					if (queues.Count > 0) { await ops.MergeBatch(queues); }
				}
				log.LogInformation($"Trade Service Operation Dispatcher ran at EST: {Util.CurrentDateTimeEST}");
			}
			catch (Exception e)
			{
				log.LogError(e, formatter, e.Message, e.StackTrace);
				throw;
			}
		}
	}
}