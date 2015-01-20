using System;
using System.Linq;
using NHibernate.Linq;
using Xunit.Extensions;

namespace Archon.Webhooks.NHibernate.Tests.Subscribing_to_an_event
{
	public class when_subscribing_to_an_event : TestFixture
	{
		Uri uri = new Uri("http://example.com/my/webhook/");

		public override void Observe()
		{
			bus.Subscribe(uri);
		}

		[Observation]
		public void should_add_webhook_subscription_to_database()
		{
			db.Query<Webhook>().Count().ShouldEqual(1);

			var hook = db.Query<Webhook>().Single();
			hook.Url.ShouldEqual(uri);
		}
	}

	[HandleExceptions]
	public class when_subscribing_to_an_event_with_null_uri : TestFixture
	{
		public override void Observe()
		{
			bus.Subscribe(null);
		}

		[Observation]
		public void should_throw_argument_null_exception()
		{
			ThrownException.ShouldBeType<ArgumentNullException>();
		}
	}
}