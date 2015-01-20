using System;
using Archon.WebApi;
using Autofac;
using Autofac.Integration.WebApi;

namespace Archon.Webhooks.Tests
{
	public class TestConfiguration : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<FakeEventBus>().As<FakeEventBus>().As<EventBus>().InstancePerMatchingLifetimeScope("application");
			builder.RegisterApiControllers(typeof(WebhookController).Assembly);
		}
	}
}