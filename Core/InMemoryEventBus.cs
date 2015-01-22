using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Archon.Webhooks
{
	public class InMemoryEventBus : EventBus
	{
		protected readonly IDictionary<Uri, Webhook> subscriptions;
		protected readonly ISet<Event> events;
		readonly HttpClient client;

		public InMemoryEventBus(HttpClient client)
		{
			this.client = client;
			subscriptions = new Dictionary<Uri, Webhook>();
			events = new HashSet<Event>();
		}

		public virtual IEnumerable<Webhook> GetSubscriptionsForCurrentUser()
		{
			return subscriptions.Values.Where(h => h.CreatedBy == Thread.CurrentPrincipal.Identity.Name);
		}

		public virtual Webhook Subscribe(Uri url)
		{
			if (url == null)
				throw new ArgumentNullException("url");

			if (!subscriptions.ContainsKey(url))
			{
				subscriptions.Add(url, new Webhook(url)
				{
					Id = subscriptions.Any() ? subscriptions.Max(s => s.Value.Id) + 1 : 1
				});
			}

			return subscriptions[url];
		}

		public virtual void Unsubscribe(int id)
		{
			foreach (var sub in subscriptions)
			{
				if (sub.Value.Id == id)
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
				events.Add(new Event(sub.Value, type, evt));
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