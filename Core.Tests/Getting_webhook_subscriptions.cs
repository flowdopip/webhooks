using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Xunit.Extensions;

namespace Archon.Webhooks.Tests.Getting_webhook_subscriptions
{
	public abstract class behaves_like_multiple_subscriptions_from_multiple_users : TestFixture
	{
		public behaves_like_multiple_subscriptions_from_multiple_users()
		{
			security.Authorize("marge.simpson");
			api.PostAsJsonAsync("/hooks/", new { url = "http://marge.simpsons.com/1" });
			api.PostAsJsonAsync("/hooks/", new { url = "http://marge.simpsons.com/2" });

			security.Authorize("homer.simpson");
			api.PostAsJsonAsync("/hooks/", new { url = "http://homer.simpsons.com/1" });
			api.PostAsJsonAsync("/hooks/", new { url = "http://homer.simpsons.com/2" });
			api.PostAsJsonAsync("/hooks/", new { url = "http://homer.simpsons.com/3" });
		}
	}

	public class when_getting_subscriptions : behaves_like_multiple_subscriptions_from_multiple_users
	{
		HttpResponseMessage response;

		public override void Observe()
		{
			response = api.GetAsync("/hooks/").Result;
		}

		[Observation]
		public void should_return_successful_result()
		{
			response.StatusCode.ShouldEqual(HttpStatusCode.OK);
		}

		[Observation]
		public void should_return_only_subscriptions_for_the_current_user()
		{
			response.Content.ShouldNotBeNull();
			IEnumerable<dynamic> results = response.Content.ReadAsAsync<IEnumerable<ExpandoObject>>().Result;

			results.Count().ShouldEqual(3);

			var urls = results.Select(h => (string)h.Url);
			urls.ShouldContain("http://homer.simpsons.com/1");
			urls.ShouldContain("http://homer.simpsons.com/2");
			urls.ShouldContain("http://homer.simpsons.com/3");
		}
	}

	public class when_getting_subscriptions_while_not_authenticated : behaves_like_multiple_subscriptions_from_multiple_users
	{
		HttpResponseMessage response;

		public when_getting_subscriptions_while_not_authenticated()
		{
			security.ClearCredentials();
		}

		public override void Observe()
		{
			response = api.GetAsync("/hooks/").Result;
		}

		[Observation]
		public void should_return_unauthorized_result()
		{
			response.StatusCode.ShouldEqual(HttpStatusCode.Unauthorized);
		}
	}

	public class when_getting_subscriptions_for_user_that_has_created_hooks_but_no_longer_has_permissions : behaves_like_multiple_subscriptions_from_multiple_users
	{
		HttpResponseMessage response;

		public when_getting_subscriptions_for_user_that_has_created_hooks_but_no_longer_has_permissions()
		{
			security.AuthenticateWithoutPermissions("homer.simpson");
		}

		public override void Observe()
		{
			response = api.GetAsync("/hooks/").Result;
		}

		[Observation]
		public void should_return_successful_result()
		{
			response.StatusCode.ShouldEqual(HttpStatusCode.OK);
		}

		[Observation]
		public void should_return_only_subscriptions_for_the_current_user()
		{
			response.Content.ShouldNotBeNull();
			IEnumerable<dynamic> results = response.Content.ReadAsAsync<IEnumerable<ExpandoObject>>().Result;

			results.Count().ShouldEqual(3);

			var urls = results.Select(h => (string)h.Url);
			urls.ShouldContain("http://homer.simpsons.com/1");
			urls.ShouldContain("http://homer.simpsons.com/2");
			urls.ShouldContain("http://homer.simpsons.com/3");
		}
	}

	public class when_getting_subscriptions_for_user_that_has_no_hooks_and_no_permissions : behaves_like_multiple_subscriptions_from_multiple_users
	{
		HttpResponseMessage response;

		public when_getting_subscriptions_for_user_that_has_no_hooks_and_no_permissions()
		{
			security.AuthenticateWithoutPermissions("lisa.simpson");
		}

		public override void Observe()
		{
			response = api.GetAsync("/hooks/").Result;
		}

		[Observation]
		public void should_return_successful_result()
		{
			response.StatusCode.ShouldEqual(HttpStatusCode.OK);
		}

		[Observation]
		public void should_return_no_subscriptions_for_the_current_user()
		{
			response.Content.ShouldNotBeNull();
			IEnumerable<dynamic> results = response.Content.ReadAsAsync<IEnumerable<ExpandoObject>>().Result;

			results.ShouldBeEmpty();
		}
	}
}