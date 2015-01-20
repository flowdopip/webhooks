using System;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Xunit;
using Xunit.Extensions;

namespace Archon.Webhooks.Tests.Subscribing_to_an_event
{
	public class when_subscribing_to_an_event : TestFixture
	{
		HttpResponseMessage response;
		Uri uri = new Uri("http://example.com/my/webhook/");

		public override void Observe()
		{
			response = api.PostAsJsonAsync("/hooks/", new
			{
				url = uri
			}).Result;
		}

		[Observation]
		public void should_return_successful_result()
		{
			response.StatusCode.ShouldEqual(HttpStatusCode.Created);
		}

		[Observation]
		public void should_return_location_of_created_webhook()
		{
			response.Headers.Location.ShouldNotBeNull();
			response.Headers.Location.ShouldEqual(new Uri("http://localhost/hooks/1"));
		}

		[Observation]
		public void should_return_webhook_data()
		{
			response.Content.ShouldNotBeNull();
			dynamic data = response.Content.ReadAsAsync<ExpandoObject>().Result;

			Assert.Equal(1, data.Id);
			Assert.Equal(uri.ToString(), data.Url);
		}

		[Observation]
		public void should_persist_webhook()
		{
			events.Subscriptions.ShouldNotBeEmpty();
			events.Subscriptions.Count().ShouldEqual(1);

			events.Subscriptions.Single().Url.ShouldEqual(uri);
		}
	}

	public class when_subscribing_to_an_event_with_a_null_url : TestFixture
	{
		HttpResponseMessage response;

		public override void Observe()
		{
			response = api.PostAsJsonAsync("/hooks/", new
			{
				url = (string)null
			}).Result;
		}

		[Observation]
		public void should_return_bad_request_result()
		{
			response.StatusCode.ShouldEqual(HttpStatusCode.BadRequest);
		}

		[Observation]
		public void should_not_persist_webhook()
		{
			events.Subscriptions.ShouldBeEmpty();
		}
	}

	public class when_subscribing_to_an_event_with_an_invalid_url : TestFixture
	{
		HttpResponseMessage response;

		public override void Observe()
		{
			response = api.PostAsJsonAsync("/hooks/", new
			{
				url = 879
			}).Result;
		}

		[Observation]
		public void should_return_bad_request_result()
		{
			response.StatusCode.ShouldEqual(HttpStatusCode.BadRequest);
		}

		[Observation]
		public void should_not_persist_webhook()
		{
			events.Subscriptions.ShouldBeEmpty();
		}
	}

	public class when_subscribing_to_an_event_with_a_missing_url : TestFixture
	{
		HttpResponseMessage response;

		public override void Observe()
		{
			response = api.PostAsJsonAsync("/hooks/", new { }).Result;
		}

		[Observation]
		public void should_return_bad_request_result()
		{
			response.StatusCode.ShouldEqual(HttpStatusCode.BadRequest);
		}

		[Observation]
		public void should_not_persist_webhook()
		{
			events.Subscriptions.ShouldBeEmpty();
		}
	}
}