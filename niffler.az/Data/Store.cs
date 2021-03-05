using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Azure.Storage.RetryPolicies;
using Newtonsoft.Json;
using Niffler.Data.ATS;
using System;
using System.Threading.Tasks;

namespace Niffler.Data
{
	/// <summary>
	/// Common object for Azure Storage operations, e.g. web queue operations.
	/// </summary>
	public class Store : IDisposable
	{
		public Store()
		{
		}

		public async static Task PushMessageToQueue(string queueName, string message, TimeSpan? delay = null)
		{
			CloudQueue queue = await GetCloudQueue(queueName);
			CloudQueueMessage queueMessage = new CloudQueueMessage(message);
			//OperationContext context = new OperationContext();
			OperationContext context = null;
			await queue.AddMessageAsync(queueMessage, null, delay, TableManager.QueueOptions, context);
		}

		public async static Task PushMessageToQueue(string queueName, object[] objects, TimeSpan? delay = null)
		{
			CloudQueue queue = await GetCloudQueue(queueName);
			OperationContext context = null;
			foreach (object o in objects)
			{
				CloudQueueMessage queueMessage = new CloudQueueMessage(Serialize(o));
				await queue.AddMessageAsync(queueMessage, null, delay, TableManager.QueueOptions, context);
			}
		}

		public async static Task UpdateQueueMessage(string queueName, string message, TimeSpan delay)
		{
			CloudQueue queue = await GetCloudQueue(queueName);
			CloudQueueMessage queueMessage = new CloudQueueMessage(message);

			QueueRequestOptions options = new QueueRequestOptions()
			{
				//RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(5), 10),
				RetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(2), 10),
				MaximumExecutionTime = TimeSpan.FromSeconds(60)
			};
			//OperationContext context = new OperationContext();
			OperationContext context = null;
			await queue.AddMessageAsync(queueMessage, null, delay, options, context);
		}

		public async static Task<CloudQueue> GetCloudQueue(string queueName)
		{
			CloudQueue queue = Cache.QueueClient.GetQueueReference(queueName);
			await queue.CreateIfNotExistsAsync();
			return queue;
		}

		public static string Serialize(object type)
		{
			return JsonConvert.SerializeObject(type);
		}

		public static T Deserialize<T>(string message)
		{
			return JsonConvert.DeserializeObject<T>(message);
		}

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

		~Store()
		{
			Dispose(false);
		}

		private bool disposed = false;
	}
}