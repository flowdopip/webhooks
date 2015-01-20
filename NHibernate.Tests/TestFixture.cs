using System;
using System.Net.Http;
using Archon.WebApi;
using Archon.Webhooks.NHibernate.Mappings;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Tool.hbm2ddl;
using Xunit.Extensions;
using NHConfiguration = NHibernate.Cfg.Configuration;

namespace Archon.Webhooks.NHibernate.Tests
{
	public abstract class TestFixture : Specification, IDisposable
	{
		static readonly NHConfiguration cfg;
		static readonly ISessionFactory factory;

		static TestFixture()
		{
			cfg = Fluently.Configure().Database(SQLiteConfiguration.Standard.InMemory()).Mappings(map =>
			{
				map.FluentMappings.AddFromAssemblyOf<WebhookMap>();
			}).BuildConfiguration();

			factory = cfg.BuildSessionFactory();
		}

		protected readonly ISession db;
		protected readonly NHibernateEventBus bus;
		protected readonly HttpClient client;
		protected readonly FakeHttpHandler fakeHandler;

		public TestFixture()
		{
			db = factory.OpenSession();
			var schema = new SchemaExport(cfg);
			schema.Execute(false, true, false, db.Connection, null);

			fakeHandler = new FakeHttpHandler();
			client = new HttpClient(fakeHandler);
			bus = new NHibernateEventBus(db, client);
		}

		public void Dispose()
		{
			if (db != null)
				db.Dispose();

			if (client != null)
				client.Dispose();
		}
	}
}