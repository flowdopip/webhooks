using System;

namespace Archon.Webhooks
{
	public class Webhook
	{
		public int Id { get; internal set; }
		public Uri Url { get; private set; }

		private Webhook() { }

		public Webhook(Uri url)
		{
			if (url == null)
				throw new ArgumentNullException("url");

			this.Url = url;
		}

		public override string ToString()
		{
			return Url.ToString();
		}
	}
}