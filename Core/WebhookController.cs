using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Archon.WebApi;

namespace Archon.Webhooks
{
	[RoutePrefix("hooks")]
	public class WebhookController : ApiController
	{
		readonly EventBus bus;

		public WebhookController(EventBus bus)
		{
			this.bus = bus;
		}

		[HttpGet]
		[Route("")]
		[AuthorizeCorrectly]
		[EnsureTrailingSlash]
		public HttpResponseMessage GetSubscriptions()
		{
			var hooks = bus.GetSubscriptionsForCurrentUser().ToArray();
			return Request.CreateResponse(hooks);
		}

		[HttpPost]
		[Route("")]
		[AuthorizeCorrectly(Roles = "Webhooks")]
		[EnsureTrailingSlash]
		public HttpResponseMessage Subscribe(NewWebhook data)
		{
			if (data == null || data.Url == null)
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Missing required Url for webhook.");

			var hook = bus.Subscribe(data.Url);

			var response = Request.CreateResponse(HttpStatusCode.Created, hook);
			response.Headers.Location = new Uri(Request.RequestUri, hook.Id.ToString());
			return response;
		}

		[HttpDelete]
		[Route("{id:int}")]
		[EnsureNoTrailingSlash]
		[AuthorizeCorrectly(Roles = "Webhooks")]
		public void Unsubscribe(int id)
		{
			bus.Unsubscribe(id);
		}

		[HttpPost]
		[Route("flush")]
		[EnsureNoTrailingSlash]
		public async Task FlushEvents()
		{
			await bus.Publish();
		}

		public class NewWebhook
		{
			public Uri Url { get; set; }
		}
	}
}