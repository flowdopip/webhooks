using System;
using Archon.Webhooks.Api;
using Autofac;
using Autofac.Integration.WebApi;

namespace Archon.Webhooks.Tests
{
	public class TestConfiguration : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<FakeEventBus>().As<FakeEventBus>().As<EventBus>().InstancePerMatchingLifetimeScope("application");
			builder.RegisterApiControllers(typeof(HookController).Assembly);
		}
	}
}