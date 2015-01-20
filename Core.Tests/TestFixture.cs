using System;
using System.Net.Http;
using System.Web.Http;
using Archon.WebApi;
using Autofac;
using Autofac.Integration.WebApi;
using Xunit.Extensions;

namespace Archon.Webhooks.Tests
{
	public abstract class TestFixture : Specification, IDisposable
	{
		static readonly IContainer container;

		static TestFixture()
		{
			var builder = new ContainerBuilder();
			builder.RegisterModule(new TestConfiguration());
			container = builder.Build();
		}

		protected readonly ILifetimeScope scope;

		protected readonly HttpServer server;
		protected readonly HttpClient api;
		protected readonly FakeHttpHandler security;
		protected readonly FakeEventBus events;

		public TestFixture()
		{
			var config = new HttpConfiguration();
			config.MapHttpAttributeRoutes();
			config.DependencyResolver = new AutofacWebApiDependencyResolver(scope = container.BeginLifetimeScope("application"));

			server = new HttpServer(config);

			api = new HttpClient(server);
			api.BaseAddress = new Uri("http://localhost/");

			events = scope.Resolve<FakeEventBus>();
		}

		public void Dispose()
		{
			if (server != null)
				server.Dispose();

			if (api != null)
				api.Dispose();

			if (scope != null)
				scope.Dispose();
		}
	}
}