using System;
using System.Threading.Tasks;

namespace Archon.Webhooks
{
	public interface EventBus
	{
		Webhook Subscribe(Uri url);
		void Unsubscribe(int id);

		void Queue(string type, object evt);

		Task Publish();
	}
}