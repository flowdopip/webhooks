using System;
using NHibernate.Linq;
using Xunit.Extensions;

namespace Archon.Webhooks.NHibernate.Tests.Unsubscribing_from_an_event
{
	public abstract class behaves_like_a_subscribed_event : TestFixture
	{
		protected int id;

		public behaves_like_a_subscribed_event()
		{
			id = bus.Subscribe(new Uri("http://example.com/my/webhook/")).Id;
		}
	}

	public class when_unsubscribing_from_an_event : behaves_like_a_subscribed_event
	{
		public override void Observe()
		{
			bus.Unsubscribe(id);
		}

		[Observation]
		public void should_remove_webhook_subscription_from_database()
		{
			db.Query<Webhook>().ShouldBeEmpty();
		}
	}
}