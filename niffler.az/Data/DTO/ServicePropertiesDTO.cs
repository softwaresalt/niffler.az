using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Niffler.Data.ATS;
using System.Collections.Generic;
using System.Linq;

namespace Niffler.Data
{
	public class ServicePropertiesDTO
	{
		public long ServiceID { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		public ServiceType ServiceType { get; set; }

		public string ServiceName { get; set; }
		public string ServiceURL { get; set; }
		public string ServiceParameterName { get; set; }
		public int ServiceParameterVersion { get; set; }
		public string ServiceParameterValue { get; set; }
		public int ServiceParameterLimit { get; set; }
		public List<ServiceLoginParameterDTO> ServiceLoginParameters { get; set; }

		public ServicePropertiesDTO()
		{ }

		public ServicePropertiesDTO(Service service, ServiceParameter parameter, List<ServiceLoginParameter> loginParameters)
		{
			this.ServiceID = service.ServiceID;
			this.ServiceName = service.Name;
			this.ServiceType = service.ServiceTypeEnum;
			this.ServiceURL = service.URL;
			this.ServiceParameterName = parameter.Name;
			this.ServiceParameterVersion = parameter.Version;
			this.ServiceParameterValue = parameter.QueryString;
			this.ServiceParameterLimit = parameter.Limit;
			this.ServiceLoginParameters = (
					from loginParam in loginParameters
					where loginParam.ServiceID == service.ServiceID
					select new ServiceLoginParameterDTO(loginParam)
			).ToList();
		}
	}
}