using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Niffler.Data.ATS;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Niffler.Data
{
	/// <summary>
	/// Encapsulates service queue attributes for in-app use.
	/// Designed to be serializable to JSON for storage queue.
	/// </summary>
	public class ServiceQueueDTO
	{
		public ServiceQueueDTO()
		{
		}

		public class TaskDTO
		{
			[JsonIgnore]
			private readonly TimeSpan startTimeDefault = new TimeSpan(9, 30, 0); //9:30AM EST

			[JsonIgnore]
			private readonly TimeSpan runByTimeDefault = new TimeSpan(10, 0, 0); //10:00AM EST

			[JsonConstructor]
			public TaskDTO() { }

			public TaskDTO(long serviceQueueID, Workflow workflow, List<WorkflowOperation> operations)
			{
				//Populate task properties
				this.AccountID = workflow.AccountID;
				this.ServiceQueueID = serviceQueueID;
				this.WorkflowID = workflow.WorkflowID;
				this.StartTimeEST = (DateTime.TryParse(workflow.StartTimeEST, out DateTime dt)) ? dt.TimeOfDay : startTimeDefault;
				this.RunByTimeEST = (DateTime.TryParse(workflow.RunByTimeEST, out dt)) ? dt.TimeOfDay : runByTimeDefault;
				this.QueueDate = Util.CurrentDateEST;
				this.QueueTime = Util.CurrentEST;
				this.TradeBalance = workflow.CurrentBalance;
				//Populate QueueItem/Operation properties
				this.QueueItem = new QueueItem(operations)
				{
					WorkflowID = workflow.WorkflowID,
					Name = workflow.Name,
					ModuleType = workflow.ModuleTypeEnum
				};
			}

			public TaskDTO(JobDTO job)
			{
				//Populate task properties
				Workflow workflow = Cache.Workflows.Find(x => x.WorkflowID == job.WorkflowID);
				ServiceQueue queue = Cache.ServiceQueues.Find(x => x.ServiceQueueID == job.ServiceQueueID);
				this.QueueItem = Store.Deserialize<QueueItem>(queue.QueueItem);

				this.AccountID = job.AccountID;
				this.ServiceQueueID = job.ServiceQueueID;
				this.WorkflowID = job.WorkflowID;
				this.StartTimeEST = (DateTime.TryParse(queue.StartTimeEST, out DateTime dt)) ? dt.TimeOfDay : startTimeDefault;
				this.RunByTimeEST = (DateTime.TryParse(queue.RunByTimeEST, out dt)) ? dt.TimeOfDay : runByTimeDefault;
				this.QueueDate = job.QueueDate;
				this.QueueTime = job.QueueTime;
				this.TradeBalance = workflow.CurrentBalance;
			}

			public long AccountID { get; set; }
			public long ServiceQueueID { get; set; }
			public DateTime QueueDate { get; set; }
			public long WorkflowID { get; set; }
			public TimeSpan StartTimeEST { get; set; }
			public TimeSpan RunByTimeEST { get; set; }
			public QueueItem QueueItem { get; set; }
			public TimeSpan? QueueTime { get; set; }
			public double TradeBalance { get; set; }
			public DateTime? CompletionDate { get; set; }

			public string SerializeQueueItem()
			{
				return JsonConvert.SerializeObject(QueueItem);
			}
		}

		/// <summary>
		/// Encapsulates a queue item and all related operations
		/// to be manage by its queue property.
		/// </summary>
		public class QueueItem
		{
			[JsonConstructor]
			public QueueItem() { }

			public QueueItem(List<WorkflowOperation> operations)
			{
				this.OperationQueue = new Queue<Operation>();
				foreach (var operation in operations.OrderBy(x => x.Ordinal))
				{
					this.OperationQueue.Enqueue(new Operation(operation));
				}
			}

			[JsonProperty(Required = Required.Always, PropertyName = "WorkflowID")]
			public long WorkflowID { get; set; }

			[JsonProperty(Required = Required.Always, PropertyName = "Name")]
			public string Name { get; set; }

			[JsonProperty(Required = Required.Always, PropertyName = "ModuleType")]
			[JsonConverter(typeof(StringEnumConverter))]
			public ModuleType ModuleType { get; set; }

			[JsonProperty(Required = Required.Always, PropertyName = "Operation")]
			public Queue<Operation> OperationQueue { get; set; }
		}

		/// <summary>
		/// Encapsulates operations configured in table storage for in-app use.
		/// </summary>
		public class Operation
		{
			[JsonConstructor]
			public Operation() { }

			public Operation(WorkflowOperation operation)
			{
				this.Ordinal = operation.Ordinal;
				this.Limit = operation.Limit;
				this.Minimum = operation.Minimum;
				this.Sequence = operation.Sequence;
				this.OperationType = operation.OperationTypeEnum;
				this.ServiceType = operation.ServiceTypeEnum;
				this.ServiceID = operation.ServiceID;
				this.ParameterID = operation.ParameterID;
				if (DateTime.TryParse(operation.RunByTimeEST, out DateTime dt)) { this.RunByTimeEST = dt.TimeOfDay; }
				if (DateTime.TryParse(operation.StartTimeEST, out dt)) { this.StartTimeEST = dt.TimeOfDay; }
			}

			[JsonProperty(Required = Required.Always, PropertyName = "Ordinal")]
			public int Ordinal { get; set; }

			[JsonProperty(Required = Required.Always, PropertyName = "Limit")]
			public int Limit { get; set; }

			[JsonProperty(Required = Required.AllowNull, PropertyName = "Minimum")]
			public int? Minimum { get; set; }

			[JsonProperty(Required = Required.Default, PropertyName = "Sequence")]
			public string Sequence { get; set; }

			[JsonProperty(Required = Required.Always, PropertyName = "StartTimeEST")]
			public TimeSpan StartTimeEST { get; set; }

			[JsonProperty(Required = Required.Always, PropertyName = "RunByTimeEST")]
			public TimeSpan RunByTimeEST { get; set; }

			[JsonProperty(Required = Required.Always, PropertyName = "ServiceType")]
			[JsonConverter(typeof(StringEnumConverter))]
			public ServiceType ServiceType { get; set; }

			[JsonProperty(Required = Required.Always, PropertyName = "OperationType")]
			[JsonConverter(typeof(StringEnumConverter))]
			public OperationType OperationType { get; set; }

			[JsonProperty(Required = Required.Always, PropertyName = "ServiceID")]
			public long ServiceID { get; set; }

			[JsonProperty(Required = Required.Default, PropertyName = "ParameterID")]
			public long? ParameterID { get; set; }
		}
	}
}