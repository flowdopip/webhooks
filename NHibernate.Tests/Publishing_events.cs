using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NHibernate.Linq;
using Xunit.Extensions;

namespace Archon.Webhooks.NHibernate.Tests.Publishing_events
{
	public abstract class behaves_like_publishing_events : TestFixture
	{
		protected HttpRequestMessage request;

		public behaves_like_publishing_events()
		{
			bus.Subscribe(new Uri("http://localhost/hello"));
			bus.Queue("thing.updated", new
			{
				hello = "world",
				one = 2
			});

			fakeHandler.Action = (req, c) =>
			{
				this.request = req;
				return Task.FromResult(req.CreateResponse(Status));
			};
		}

		protected virtual HttpStatusCode Status
		{
			get { return HttpStatusCode.NoContent; }
		}
	}

	public class when_successfully_publishing_events : behaves_like_publishing_events
	{
		public override void Observe()
		{
			bus.Publish().Wait();
		}

		[Observation]
		public void should_clear_events_for_each_subscriber()
		{
			db.Query<Event>().ShouldBeEmpty();
		}

		[Observation]
		public void should_have_published_event_to_webhook()
		{
			request.ShouldNotBeNull();
		}
	}

	public class when_webhook_returns_unsuccessful_response : behaves_like_publishing_events
	{
		protected override HttpStatusCode Status
		{
			get { return HttpStatusCode.InternalServerError; }
		}

		public override void Observe()
		{
			bus.Publish().Wait();
		}

		[Observation]
		public void should_have_published_event_to_webhook()
		{
			request.ShouldNotBeNull();
		}

		[Observation]
		public void should_not_clear_events_for_each_subscriber()
		{
			db.Query<Event>().Count().ShouldEqual(1);
		}

		[Observation]
		public void should_mark_event_with_failed_attempt()
		{
			var evt = db.Query<Event>().Single();
			evt.LastAttempt.ShouldNotBeNull();
		}
	}
}