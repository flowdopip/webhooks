using FluentNHibernate.Mapping;
using NHibernate.Type;

namespace Archon.Webhooks.NHibernate.Mappings
{
	public class WebhookMap : ClassMap<Webhook>
	{
		public WebhookMap()
		{
			Not.LazyLoad();
			Schema("web");
			Table("Hooks");

			Id(x => x.Id).Column("WebHookId").GeneratedBy.Identity();
			Map(x => x.Url).CustomType<UriType>().Unique();
		}
	}
}