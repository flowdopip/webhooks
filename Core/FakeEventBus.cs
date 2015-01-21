using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Archon.Webhooks
{
	public class FakeEventBus : EventBus
	{
		readonly IList<KeyValuePair<string, object>> events;

		public IEnumerable<KeyValuePair<string, object>> Events
		{
			get { return events; }
		}

		public FakeEventBus()
		{
			events = new List<KeyValuePair<string, object>>();
		}

		public Webhook Subscribe(Uri url)
		{
			throw new NotSupportedException();
		}

		public void Unsubscribe(int id)
		{
			throw new NotSupportedException();
		}

		public void Queue(string type, object evt)
		{
			events.Add(new KeyValuePair<string, object>(type, evt));
		}

		public Task Publish()
		{
			Clear();
			return Task.FromResult(true);
		}

		public void Clear()
		{
			events.Clear();
		}

		public object AssertEvent(string type)
		{
			if (String.IsNullOrWhiteSpace(type))
				throw new ArgumentNullException("type");

			foreach (var evt in events)
			{
				if (evt.Key == type)
				{
					return evt.Value;
				}
			}

			throw new KeyNotFoundException(String.Format("Could not find published event of type '{0}'.", type));
		}
	}
}