using System;
using System.Linq;
using NHibernate.Linq;
using Xunit;
using Xunit.Extensions;

namespace Archon.Webhooks.NHibernate.Tests.Queueing_an_event
{
	public abstract class behaves_like_multiple_subscribers : TestFixture
	{
		public behaves_like_multiple_subscribers()
		{
			bus.Subscribe(new Uri("http://localhost/hello"));
			bus.Subscribe(new Uri("http://example.com/world"));
		}
	}

	public class when_queueing_an_event : behaves_like_multiple_subscribers
	{
		public override void Observe()
		{
			bus.Queue("thing.updated", new
			{
				hello = "world",
				one = 2
			});
		}

		[Observation]
		public void should_save_event_for_each_subscriber()
		{
			db.Query<Event>().Count().ShouldEqual(2);
		}

		[Observation]
		public void should_save_event_payload_and_type()
		{
			var evt = db.Query<Event>().First();

			evt.Type.ShouldEqual("thing.updated");
			dynamic payload = evt.Payload;

			Assert.Equal("world", payload.hello);
			Assert.Equal(2, payload.one);
		}
	}

	public class when_queueing_an_event_with_no_subscribers : TestFixture
	{
		public override void Observe()
		{
			bus.Queue("thing.updated", new
			{
				hello = "world",
				one = 2
			});
		}

		[Observation]
		public void should_not_save_any_events()
		{
			db.Query<Event>().ShouldBeEmpty();
		}
	}

	[HandleExceptions]
	public class when_queueing_an_event_with_null_type : behaves_like_multiple_subscribers
	{
		public override void Observe()
		{
			bus.Queue(null, new
			{
				hello = "world",
				one = 2
			});
		}

		[Observation]
		public void should_throw_argument_null_exception()
		{
			ThrownException.ShouldBeType<ArgumentNullException>();
		}

		[Observation]
		public void should_not_save_event()
		{
			db.Query<Event>().ShouldBeEmpty();
		}
	}

	[HandleExceptions]
	public class when_queueing_an_event_with_null_payload : behaves_like_multiple_subscribers
	{
		public override void Observe()
		{
			bus.Queue("thing.updated", null);
		}

		[Observation]
		public void should_throw_argument_null_exception()
		{
			ThrownException.ShouldBeType<ArgumentNullException>();
		}

		[Observation]
		public void should_not_save_event()
		{
			db.Query<Event>().ShouldBeEmpty();
		}
	}
}