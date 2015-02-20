using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Archon.WebApi;

namespace Archon.Webhooks.Api
{
	[RoutePrefix("hooks/{hookId:int}/events")]
	public class EventController : ApiController
	{
		readonly EventBus bus;

		public EventController(EventBus bus)
		{
			this.bus = bus;
		}
	}
}