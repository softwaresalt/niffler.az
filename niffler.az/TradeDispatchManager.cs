using Microsoft.Extensions.Logging;
using Niffler.Data;
using Niffler.Interfaces;
using Niffler.Modules;
using System.Threading.Tasks;

namespace Niffler
{
	/// <summary>
	/// Determines which trade module to instantiate and runs it;
	/// module will handle configured operations.
	/// </summary>
	public class TradeDispatchManager : WorkflowManagerBase, IWorkflowManager
	{
		public TradeDispatchManager(ILogger log) : base(log)
		{
		}

		public TradeDispatchDTO Job { get; private set; }

		public async Task RunAsync<T>(T dto)
		{
			Job = dto as TradeDispatchDTO;
			bool isWorkDone = false;
			IModule module = null;
			switch (Job.ModuleType)
			{
				case ModuleType.EasyInOut:
					module = new EasyInOut(new ServiceManager(Logger));
					break;

				case ModuleType.Niffler:
					module = new NifflerMod(new ServiceManager(Logger));
					break;
			}
			if (module != null)
			{
				isWorkDone = await module.RunAsync(Job);
				if (module.DTO != null)
				{
					this.Job = module.DTO as TradeDispatchDTO;
				}
				else
				{
					this.Logger.LogWarning("TradeDispatchManager: Attempted to store NULL module.DTO to TradeDispatchDTO; original DTO: {dto}", Store.Serialize(Job));
				}
			}
			if (!isWorkDone)
			{
				await Util.RetryOnExceptionAsync(async () => await Store.PushMessageToQueue(Cache.TradeDispatchMonitorQueueName, Store.Serialize(Job), module.Delay), this.Logger);
			}
		}
	}
}