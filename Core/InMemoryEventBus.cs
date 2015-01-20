using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Archon.Webhooks
{
	public class InMemoryEventBus : EventBus
	{
		protected readonly ISet<Webhook> subscriptions;
		protected readonly ISet<Event> events;
		readonly HttpClient client;

		public InMemoryEventBus(HttpClient client)
		{
			subscriptions = new HashSet<Webhook>();
			events = new HashSet<Event>();
			this.client = client;
		}

		public virtual Webhook Subscribe(Uri url)
		{
			if (url == null)
				throw new ArgumentNullException("url");

			var hook = new Webhook(url)
			{
				Id = subscriptions.Any() ? subscriptions.Max(s => s.Id) + 1 : 1
			};

			subscriptions.Add(hook);
			return hook;
		}

		public virtual void Unsubscribe(int id)
		{
			foreach (var sub in subscriptions)
			{
				if (sub.Id == id)
				{
					subscriptions.Remove(sub);
					break;
				}
			}
		}

		public virtual void Queue(string type, object evt)
		{
			if (String.IsNullOrWhiteSpace(type))
				throw new ArgumentNullException("type");

			if (evt == null)
				throw new ArgumentNullException("evt");

			foreach (var sub in subscriptions)
				events.Add(new Event(sub, type, evt));
		}

		public virtual async Task Publish()
		{
			foreach (var evt in events.ToArray())
			{
				var result = await client.PublishEvent(evt);

				if (result.IsSuccess)
					events.Remove(evt);
			}
		}
	}
}