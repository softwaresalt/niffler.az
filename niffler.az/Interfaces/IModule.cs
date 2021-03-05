using System;
using System.Threading.Tasks;

namespace Niffler.Interfaces
{
	public interface IModule
	{
		Task<bool> RunAsync<T>(T task);

		TimeSpan Delay { get; set; }
		object DTO { get; set; }
	}

	public abstract class Module<IServiceManager>
	{
		public Module(IServiceManager serviceManager)
		{
			this.Manager = serviceManager;
		}

		public IServiceManager Manager { get; private set; }
		public bool ClosePositions { get; internal set; } = false;
	}
}