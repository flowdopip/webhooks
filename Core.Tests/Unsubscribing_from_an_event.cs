﻿using System;
using System.Linq;
using System.Dynamic;
using System.Net;
using System.Net.Http;
using Archon.WebApi;
using Xunit.Extensions;

namespace Archon.Webhooks.Tests.Unsubscribing_from_an_event
{
	public abstract class behaves_like_existing_subscription : TestFixture
	{
		protected readonly long subscriptionId;

		public behaves_like_existing_subscription()
		{
			var response = api.PostAsJsonAsync("/hooks/", new
			{
				url = "http://example.com/my/webhook/"
			}).Result;

			response.EnsureSuccess().Wait();
			subscriptionId = ((dynamic)response.Content.ReadAsAsync<ExpandoObject>().Result).Id;
		}
	}

	public class when_unsubscribing_from_an_event : behaves_like_existing_subscription
	{
		HttpResponseMessage response;

		public override void Observe()
		{
			response = api.DeleteAsync("/hooks/" + subscriptionId).Result;
		}

		[Observation]
		public void should_return_successful_result()
		{
			response.StatusCode.ShouldEqual(HttpStatusCode.NoContent);
		}

		[Observation]
		public void should_remove_persisted_webhook()
		{
			events.Subscriptions.ShouldBeEmpty();
		}
	}

	public class when_unsubscribing_from_an_event_that_does_not_exist : behaves_like_existing_subscription
	{
		HttpResponseMessage response;

		public override void Observe()
		{
			response = api.DeleteAsync("/hooks/" + 98745).Result;
		}

		[Observation]
		public void should_still_return_successful_result()
		{
			response.StatusCode.ShouldEqual(HttpStatusCode.NoContent);
		}

		[Observation]
		public void should_not_remove_persisted_webhook()
		{
			events.Subscriptions.ShouldNotBeEmpty();
		}
	}

	public class when_unsubscribing_from_an_event_while_not_authenticated : behaves_like_existing_subscription
	{
		HttpResponseMessage response;

		public when_unsubscribing_from_an_event_while_not_authenticated()
		{
			security.ClearCredentials();
		}

		public override void Observe()
		{
			response = api.DeleteAsync("/hooks/" + subscriptionId).Result;
		}

		[Observation]
		public void should_return_unauthorized_result()
		{
			response.StatusCode.ShouldEqual(HttpStatusCode.Unauthorized);
		}

		[Observation]
		public void should_not_remove_persisted_webhook()
		{
			events.Subscriptions.Count().ShouldEqual(1);
		}
	}

	public class when_unsubscribing_from_an_event_while_not_authorized : behaves_like_existing_subscription
	{
		HttpResponseMessage response;

		public when_unsubscribing_from_an_event_while_not_authorized()
		{
			security.AuthenticateWithoutPermissions("homer.simpson");
		}

		public override void Observe()
		{
			response = api.DeleteAsync("/hooks/" + subscriptionId).Result;
		}

		[Observation]
		public void should_return_forbidden_result()
		{
			response.StatusCode.ShouldEqual(HttpStatusCode.Forbidden);
		}

		[Observation]
		public void should_not_remove_persisted_webhook()
		{
			events.Subscriptions.Count().ShouldEqual(1);
		}
	}
}