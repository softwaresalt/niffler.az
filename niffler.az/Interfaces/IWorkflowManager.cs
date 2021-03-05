using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Niffler.Interfaces
{
	public interface IWorkflowManager
	{
		Task RunAsync<T>(T dto);
	}

	/// <summary>
	/// Includes base implementation requirements for Workflow Manager class
	/// </summary>
	public abstract class WorkflowManagerBase
	{
		public WorkflowManagerBase(ILogger log)
		{
			this.Logger = log;
		}

		public ILogger Logger { get; private set; }
	}
}