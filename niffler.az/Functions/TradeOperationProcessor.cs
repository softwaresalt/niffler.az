using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Niffler.Data;
using Niffler.Interfaces;
using System;
using System.Threading.Tasks;

namespace Niffler
{
	/// <summary>
	/// TradeOperationProcessor: serverless function triggered off of "tradeoperation" queue messages
	/// </summary>
	public static class TradeOperationProcessor
	{
		[FunctionName("TradeOperationProcessor")]
		public static async Task RunAsync([QueueTrigger("tradeoperations")] string message, ILogger log)
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
				catch (Exception e)
				{ throw; }
				finally
				{
					Cache.CacheInitSemaphore.Release();
				}
			}
			JobDTO job = new JobDTO();
			bool isProcessEngaged = false;
			try
			{
				job = Store.Deserialize<JobDTO>(message);
				if (!Cache.OperationInProcess.ContainsKey(job.ServiceQueueID) && Cache.OperationInProcess.TryAdd(job.ServiceQueueID, true)) { isProcessEngaged = true; }
				if (Cache.OperationInProcess.ContainsKey(job.ServiceQueueID) && !Cache.OperationInProcess[job.ServiceQueueID]) { Cache.OperationInProcess[job.ServiceQueueID] = true; isProcessEngaged = true; }
				if (isProcessEngaged)
				{
					ServiceQueueDTO.TaskDTO task = new ServiceQueueDTO.TaskDTO(job);
					IWorkflowManager wf = new WorkflowManager(log);
					await wf.RunAsync(task);
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
				if (isProcessEngaged && Cache.OperationInProcess.ContainsKey(job.ServiceQueueID)) { Cache.OperationInProcess[job.ServiceQueueID] = false; }
			}
		}
	}
}