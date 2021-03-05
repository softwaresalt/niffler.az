using Newtonsoft.Json;
using System;

namespace Niffler.Data
{
	/// <summary>
	/// Encapsulates job/task attributes for in-app use.
	/// Designed to be serializable to JSON for storage queue.
	/// </summary>
	public class JobDTO
	{
		[JsonConstructor]
		public JobDTO() { }

		public JobDTO(ServiceQueueDTO.TaskDTO task)
		{
			this.AccountID = task.AccountID;
			this.ServiceQueueID = task.ServiceQueueID;
			this.WorkflowID = task.WorkflowID;
			this.QueueDate = task.QueueDate;
			this.QueueTime = task.QueueTime;
		}

		[JsonProperty(Required = Required.Always, PropertyName = "AccountID")]
		public long AccountID { get; set; }

		[JsonProperty(Required = Required.Always, PropertyName = "ServiceQueueID")]
		public long ServiceQueueID { get; set; }

		[JsonProperty(Required = Required.Always, PropertyName = "WorkflowID")]
		public long WorkflowID { get; set; }

		[JsonProperty(Required = Required.Always, PropertyName = "QueueDate")]
		public DateTime QueueDate { get; set; }

		[JsonProperty(Required = Required.Always, PropertyName = "QueueTime")]
		public TimeSpan? QueueTime { get; set; }
	}
}