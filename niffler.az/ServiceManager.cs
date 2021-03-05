using Alpaca.Markets;
using Microsoft.Extensions.Logging;
using Niffler.Data;
using Niffler.Data.ATS;
using Niffler.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Niffler
{
	/// <summary>
	/// Uses table parameters to log into on-line services to fetch
	/// screen and quote data.
	/// </summary>
	public class ServiceManager : IServiceManager
	{
		public const int RetryTimes = 3;

		public ServiceManager(ILogger log)
		{
			this.Logger = log;
		}

		public ILogger Logger { get; set; }

		public async Task<List<T>> RunService<T>(ServiceQueueDTO.Operation operation, long workflowID, DateTime queueDate, long serviceQueueID, string symbol, int quoteLimit = 100)
		{
			ServiceTypeMappingHelper mappingHelper;
			string content = String.Empty;
			IReadOnlyList<IAgg> quotes;
			var service = Cache.Services.Find(x => x.ServiceID == operation.ServiceID);
			var serviceParams = (Cache.ServiceParameters.Exists(x => x.ServiceID == operation.ServiceID && x.ParameterID == operation.ParameterID && x.IsEnabled)) ?
				Cache.ServiceParameters.Find(x => x.ServiceID == operation.ServiceID && x.ParameterID == operation.ParameterID && x.IsEnabled) : new ServiceParameter();
			var serviceLoginParams = Cache.ServiceLoginParameters.FindAll(x => x.ServiceID == service.ServiceID);
			ServicePropertiesDTO serviceProperties = new ServicePropertiesDTO(service, serviceParams, serviceLoginParams);

			PositionType positionType = (operation.OperationType == OperationType.MarketBuy) ? PositionType.Opened : PositionType.Closed;
			ServicePostParameterDTO postParams = new ServicePostParameterDTO(serviceProperties);
			mappingHelper = new ServiceTypeMappingHelper(this.Logger, operation.ServiceType);
			mappingHelper.fieldMap.SetFieldValues(FieldMapping.Workflow, workflowID.ToString());
			mappingHelper.fieldMap.SetFieldValues(FieldMapping.ServiceQueue, serviceQueueID.ToString());
			switch (operation.ServiceType)
			{
				case ServiceType.StockScreener:
					content = await Util.GetRealtimeScreenData(postParams);
					break;

				case ServiceType.StockQuote:
					quotes = await Util.RetryOnExceptionWithReturnAsync(async () => await Util.GetRealtimeQuoteData(symbol, TimeFrame.Minute, quoteLimit), this.Logger);
					return quotes.ToList() as List<T>;
			}
			content = mappingHelper.AddFieldValueToContent(content);
			return await mappingHelper.GenerateEntityList<T>(typeof(T), content, workflowID, queueDate);
		}
	}
}