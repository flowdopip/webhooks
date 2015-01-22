using System;
using System.Threading;

namespace Archon.Webhooks
{
	public class Webhook
	{
		public int Id { get; internal set; }
		public Uri Url { get; private set; }

		public string CreatedBy { get; private set; }
		public DateTime CreatedOn { get; private set; }

		private Webhook() { }

		public Webhook(Uri url)
		{
			if (url == null)
				throw new ArgumentNullException("url");

			this.Url = url;

			this.CreatedBy = Thread.CurrentPrincipal.Identity.Name;
			this.CreatedOn = DateTime.UtcNow;
		}

		public override string ToString()
		{
			return Url.ToString();
		}
	}
}