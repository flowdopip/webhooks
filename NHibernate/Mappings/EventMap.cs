using FluentNHibernate.Mapping;
using NHibernate.Type;

namespace Archon.Webhooks.NHibernate.Mappings
{
	public class EventMap : ClassMap<Event>
	{
		public EventMap()
		{
			Not.LazyLoad();
			Schema("web");
			Table("Events");

			Id(x => x.Id).Column("WebEventId").GeneratedBy.Identity();
			References(x => x.Hook).Column("WebHookId");

			Map(x => x.Type).Length(50);
			Map(x => x.Payload).CustomType<JsonType>();

			Map(x => x.CreatedOn).CustomType<LocalDateTimeType>().Not.Nullable();
			Map(x => x.LastAttempt).CustomType<LocalDateTimeType>();

			Map(x => x.ErrorMessage).Length(10000);
		}
	}
}