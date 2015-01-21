using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Archon.Webhooks
{
	public class FakeEventBus : InMemoryEventBus
	{
		readonly ISet<Event> publishedEvents;

		public IEnumerable<Webhook> Subscriptions
		{
			get { return subscriptions; }
		}

		public IEnumerable<Event> Events
		{
			get { return events; }
		}

		public IEnumerable<Event> PublishedEvents
		{
			get { return publishedEvents; }
		}

		public FakeEventBus()
			: base(null)
		{
			publishedEvents = new HashSet<Event>();
		}

		public void Clear()
		{
			events.Clear();
			publishedEvents.Clear();
		}

		public override void Queue(string type, object evt)
		{
			//for testing purposes, only add a single event for each queued event rather than one for each subscribed webhook
			events.Add(new Event(new Webhook(new Uri("http://example.com/fake")), type, evt));
		}

		public override Task Publish()
		{
			foreach (var evt in events)
				publishedEvents.Add(evt);

			events.Clear();

			return Task.FromResult(true);
		}

		public Event AssertEvent(string type)
		{
			foreach (var evt in PublishedEvents)
			{
				if (evt.Type == type)
				{
					return evt;
				}
			}

			throw new KeyNotFoundException(String.Format("Could not find published event of type '{0}'.", type));
		}
	}
}