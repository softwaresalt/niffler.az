using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Niffler.Modules
{
	public class Mail : IDisposable
	{
		public Mail()
		{
		}

		public Mail(string host, int port, bool useSSL = true)
		{
			this.HostAddress = host;
			this.Port = port;
			this.UseSSL = useSSL;
		}

		private NetworkCredential credentials = null;
		private MailAddress addressFrom = null;
		public int Port { get; set; } = 587;
		public bool UseSSL { get; set; } = true;
		public string HostAddress { get; set; }
		public Exception Error { get; set; }
		public string Subject { get; set; } = String.Empty;
		public string Body { get; set; } = null;

		public void SetCredentials(string userName, string Password) => credentials = new NetworkCredential(userName, Password);

		public void SetFromAddress(string address, string displayName) => addressFrom = new MailAddress(address, displayName, Encoding.ASCII);

		public async Task<SendStatus> SendToAsync(string[] toAddresses)
		{
			SendStatus status = SendStatus.None;
			try
			{
				if (toAddresses.Length == 0) { status = SendStatus.InvalidConfig; return status; }
				using (MailMessage message = new MailMessage())
				using (SmtpClient client = new SmtpClient(HostAddress, Port))
				{
					MailAddress addressTo = new MailAddress(toAddresses[0]);
					client.EnableSsl = UseSSL;
					client.Credentials = credentials;
					client.DeliveryMethod = SmtpDeliveryMethod.Network;
					client.DeliveryFormat = SmtpDeliveryFormat.SevenBit;
					message.From = addressFrom;
					foreach (string address in toAddresses) { message.To.Add(address); }
					message.Subject = Subject;
					message.SubjectEncoding = Encoding.ASCII;
					message.Body = Body;
					message.BodyEncoding = Encoding.ASCII;
					message.IsBodyHtml = false;
					if (IsValidConfig(client, message))
					{
						await client.SendMailAsync(message);
						status = SendStatus.OK;
					}
					else
					{
						status = SendStatus.InvalidConfig;
					}
				}
			}
			catch (Exception e)
			{
				Error = e;
				status = SendStatus.Error;
			}
			return status;
		}

		private bool IsValidConfig(SmtpClient client, MailMessage message)
		{
			bool isValid = false;
			if (client.Credentials != null &&
				!String.IsNullOrEmpty(HostAddress) &&
				!String.IsNullOrEmpty(Subject) &&
				addressFrom != null &&
				!String.IsNullOrEmpty(addressFrom.Address) &&
				!String.IsNullOrEmpty(message.To.First().Address)
			)
			{ isValid = true; }
			return isValid;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				disposed = true;
			}
		}

		~Mail()
		{
			Dispose(false);
		}

		private bool disposed = false;
	}

	public enum SendStatus
	{
		None,
		OK,
		Error,
		InvalidConfig
	}
}