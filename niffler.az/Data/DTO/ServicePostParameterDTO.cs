namespace Niffler.Data
{
	public class ServicePostParameterDTO
	{
		public ServicePostParameterDTO(ServicePropertiesDTO serviceProperties)
		{
			url = string.Concat(serviceProperties.ServiceURL, serviceProperties.ServiceParameterValue);
			if (serviceProperties.ServiceLoginParameters.Count > 0)
			{
				loginURL = serviceProperties.ServiceLoginParameters
						.Find(x => x.ParameterType == ServiceLoginParameterType.LoginURL).ParameterValue;
				loginSubmitURL = serviceProperties.ServiceLoginParameters
						.Find(x => x.ParameterType == ServiceLoginParameterType.LoginSubmitURL).ParameterValue;
				loginUsernameFormField = serviceProperties.ServiceLoginParameters
						.Find(x => x.ParameterType == ServiceLoginParameterType.LoginUsernameFormField).ParameterValue;
				loginUsername = serviceProperties.ServiceLoginParameters
						.Find(x => x.ParameterType == ServiceLoginParameterType.LoginUsername).ParameterValue;
				loginPasswordFormField = serviceProperties.ServiceLoginParameters
						.Find(x => x.ParameterType == ServiceLoginParameterType.LoginPasswordFormField).ParameterValue;
				loginPassword = serviceProperties.ServiceLoginParameters
						.Find(x => x.ParameterType == ServiceLoginParameterType.LoginPassword).ParameterValue;
				loginRememberFormField = serviceProperties.ServiceLoginParameters
						.Find(x => x.ParameterType == ServiceLoginParameterType.LoginRememberThisComputerFormField).ParameterValue;
				loginRemember = serviceProperties.ServiceLoginParameters
						.Find(x => x.ParameterType == ServiceLoginParameterType.LoginRememberThisComputer).ParameterValue;
			}
		}

		public string url;
		public string loginURL;
		public string loginSubmitURL;
		public string loginUsernameFormField;
		public string loginUsername;
		public string loginPasswordFormField;
		public string loginPassword;
		public string loginRememberFormField;
		public string loginRemember;
	}
}