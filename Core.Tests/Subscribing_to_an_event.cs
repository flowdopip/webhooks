using System;
using Archon.WebApi;
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

	public class when_subscribing_to_an_event_with_the_same_url : TestFixture
	{
		HttpResponseMessage response;
		long subId;

		public when_subscribing_to_an_event_with_the_same_url()
		{
			var resp = api.Send(CreateRequest());
			resp.EnsureSuccess().Wait();

			dynamic data = resp.Content.ReadAsAsync<ExpandoObject>().Result;
			subId = data.Id;
		}

		HttpRequestMessage CreateRequest()
		{
			return new HttpRequestMessage(HttpMethod.Post, "/hooks/").WithJsonContent(new
			{
				url = "http://example.com/my/webhook/"
			});
		}

		public override void Observe()
		{
			response = api.Send(CreateRequest());
		}

		[Observation]
		public void should_return_successful_result()
		{
			response.StatusCode.ShouldEqual(HttpStatusCode.Created);
		}

		[Observation]
		public void should_return_existing_webhook_data()
		{
			response.Content.ShouldNotBeNull();
			dynamic data = response.Content.ReadAsAsync<ExpandoObject>().Result;
			Assert.Equal(subId, data.Id);
		}

		[Observation]
		public void should_return_location_of_created_webhook()
		{
			response.Headers.Location.ShouldNotBeNull();
			response.Headers.Location.ShouldEqual(new Uri("http://localhost/hooks/" + subId));
		}

		[Observation]
		public void should_not_persist_another_webhook()
		{
			events.Subscriptions.ShouldNotBeEmpty();
			events.Subscriptions.Count().ShouldEqual(1);
		}
	}

	public abstract class behaves_like_failing_to_subscribe_to_an_event : TestFixture
	{
		protected HttpResponseMessage response;

		protected abstract HttpStatusCode ExpectedResult { get; }

		[Observation]
		public void should_return_expected_invalid_result()
		{
			response.StatusCode.ShouldEqual(ExpectedResult);
		}

		[Observation]
		public void should_not_persist_webhook()
		{
			events.Subscriptions.ShouldBeEmpty();
		}
	}

	public class when_subscribing_to_an_event_with_a_null_url : behaves_like_failing_to_subscribe_to_an_event
	{
		public override void Observe()
		{
			response = api.PostAsJsonAsync("/hooks/", new
			{
				url = (string)null
			}).Result;
		}

		protected override HttpStatusCode ExpectedResult
		{
			get { return HttpStatusCode.BadRequest; }
		}
	}

	public class when_subscribing_to_an_event_with_an_invalid_url : behaves_like_failing_to_subscribe_to_an_event
	{
		public override void Observe()
		{
			response = api.PostAsJsonAsync("/hooks/", new
			{
				url = 879
			}).Result;
		}

		protected override HttpStatusCode ExpectedResult
		{
			get { return HttpStatusCode.BadRequest; }
		}
	}

	public class when_subscribing_to_an_event_with_a_missing_url : behaves_like_failing_to_subscribe_to_an_event
	{
		public override void Observe()
		{
			response = api.PostAsJsonAsync("/hooks/", new { }).Result;
		}

		protected override HttpStatusCode ExpectedResult
		{
			get { return HttpStatusCode.BadRequest; }
		}
	}

	public class when_subscribing_to_an_event_without_being_authenticated : behaves_like_failing_to_subscribe_to_an_event
	{
		public when_subscribing_to_an_event_without_being_authenticated()
		{
			security.ClearCredentials();
		}

		public override void Observe()
		{
			response = api.PostAsJsonAsync("/hooks/", new
			{
				url = "http://example.com/"
			}).Result;
		}

		protected override HttpStatusCode ExpectedResult
		{
			get { return HttpStatusCode.Unauthorized; }
		}
	}

	public class when_subscribing_to_an_event_without_being_authorized : behaves_like_failing_to_subscribe_to_an_event
	{
		public when_subscribing_to_an_event_without_being_authorized()
		{
			security.AuthenticateWithoutPermissions("homer.simpson");
		}

		public override void Observe()
		{
			response = api.PostAsJsonAsync("/hooks/", new
			{
				url = "http://example.com/"
			}).Result;
		}

		protected override HttpStatusCode ExpectedResult
		{
			get { return HttpStatusCode.Forbidden; }
		}
	}
}