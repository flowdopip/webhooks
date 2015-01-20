using System;
using System.Net.Http;
using System.Threading.Tasks;
using log4net;
using NHibernate;
using NHibernate.Linq;

namespace Archon.Webhooks.NHibernate
{
	public class NHibernateEventBus : EventBus
	{
		static readonly ILog Log = LogManager.GetLogger(typeof(NHibernateEventBus));

		readonly ISession db;
		readonly HttpClient client;

		public NHibernateEventBus(ISession db, HttpClient client)
		{
			this.db = db;
			this.client = client;
		}

		public Webhook Subscribe(Uri url)
		{
			if (url == null)
				throw new ArgumentNullException("url");

			ITransaction tx = null;
			Webhook hook;

			try
			{
				if (db.Transaction == null || !db.Transaction.IsActive)
					tx = db.BeginTransaction();

				hook = new Webhook(url);
				db.Save(hook);

				if (tx != null)
					tx.Commit();
			}
			finally
			{
				if (tx != null)
					tx.Dispose();
			}

			return hook;
		}

		public void Unsubscribe(int id)
		{
			ITransaction tx = null;

			try
			{
				if (db.Transaction == null || !db.Transaction.IsActive)
					tx = db.BeginTransaction();

				db.CreateQuery("delete Webhook where Id = ?")
					.SetInt32(0, id)
					.ExecuteUpdate();

				if (tx != null)
					tx.Commit();
			}
			finally
			{
				if (tx != null)
					tx.Dispose();
			}
		}

		public void Queue(string type, object evt)
		{
			ITransaction tx = null;

			try
			{
				if (db.Transaction == null || !db.Transaction.IsActive)
					tx = db.BeginTransaction();

				foreach (var hook in db.Query<Webhook>())
					db.Save(new Event(hook, type, evt));

				if (tx != null)
					tx.Commit();
			}
			finally
			{
				if (tx != null)
					tx.Dispose();
			}
		}

		public async Task Publish()
		{
			foreach (var evt in db.Query<Event>())
			{
				using (var tx = db.BeginTransaction())
				{
					var result = await client.PublishEvent(evt);

					if (result.IsSuccess)
					{
						db.Delete(evt);
						Log.Info(result.Message);
					}
					else
					{
						Log.Error(result.Message);
					}

					tx.Commit();
				}
			}
		}
	}
}