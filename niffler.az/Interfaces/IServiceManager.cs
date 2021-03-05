using Microsoft.Extensions.Logging;
using Niffler.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Niffler.Interfaces
{
	public interface IServiceManager
	{
		ILogger Logger { get; set; }

		Task<List<T>> RunService<T>(ServiceQueueDTO.Operation operation, long workflowID, DateTime queueDate, long serviceQueueID, string symbol, int quoteLimit = 100);
	}
}