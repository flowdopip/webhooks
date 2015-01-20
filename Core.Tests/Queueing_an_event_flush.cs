using System;
using System.Net.Http;
using System.Threading;
using Archon.Webhooks.Tests.Unsubscribing_from_an_event;
using Xunit.Extensions;

namespace Archon.Webhooks.Tests.Queueing_an_event_flush
{
	public abstract class behaves_like_a_queued_event : behaves_like_existing_subscription
	{
		public behaves_like_a_queued_event()
		{
			events.Queue("something.happened", new
			{
				hello = "world",
				one = 2
			});
		}
	}

	public class when_queueing_an_event_flush : behaves_like_a_queued_event
	{
		public override void Observe()
		{
			api.QueueAsyncEventFlush();
		}

		[Observation]
		public void should_flush_the_events_from_the_queue()
		{
			events.Events.ShouldBeEmpty();
			events.PublishedEvents.ShouldNotBeEmpty();
		}
	}
}