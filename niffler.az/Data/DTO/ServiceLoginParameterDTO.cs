using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Niffler.Data.ATS;

namespace Niffler.Data
{
	public class ServiceLoginParameterDTO
	{
		[JsonConverter(typeof(StringEnumConverter))]
		public ServiceLoginParameterType ParameterType { get; set; }

		public string ParameterValue { get; set; }

		public ServiceLoginParameterDTO()
		{
		}

		public ServiceLoginParameterDTO(ServiceLoginParameter parameter)
		{
			this.ParameterType = parameter.eServiceLoginParameterType;
			this.ParameterValue = parameter.ParameterValue;
		}
	}
}