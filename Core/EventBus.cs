using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Archon.Webhooks
{
	public interface EventBus
	{
		IEnumerable<Webhook> GetSubscriptionsForCurrentUser();
		IEnumerable<Event> GetEventsForSubscription(int hookId);

		Webhook Subscribe(Uri url);
		void Unsubscribe(int id);

		void Queue(string type, object evt);

		Task Publish();
	}
}