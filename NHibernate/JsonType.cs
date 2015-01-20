using System;
using System.Data;
using Newtonsoft.Json;
using NHibernate;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;

namespace Archon.Webhooks.NHibernate
{
	[Serializable]
	public class JsonType : IUserType
	{
		public JsonSerializerSettings Settings { get; set; }

		public JsonType()
		{
			Settings = JsonConvert.DefaultSettings != null ? JsonConvert.DefaultSettings() : new JsonSerializerSettings();
			Settings.ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor;
		}

		public object Assemble(object cached, object owner)
		{
			if (cached == null) return null;
			return DeepCopy(cached);
		}

		public object DeepCopy(object value)
		{
			var cloneable = value as ICloneable;
			if (cloneable != null)
				return cloneable.Clone();

			return value;
		}

		public object Disassemble(object value)
		{
			if (value == null) return null;
			return DeepCopy(value);
		}

		public bool IsMutable
		{
			get { return true; }
		}

		public object NullSafeGet(IDataReader dr, string[] names, object owner)
		{
			if (dr == null) return null;

			string value = (string)NHibernateUtil.AnsiString.NullSafeGet(dr, names[0], null, owner);

			if (String.IsNullOrEmpty(value)) return null;

			return JsonConvert.DeserializeObject(value, Settings);
		}

		public void NullSafeSet(IDbCommand cmd, object value, int index)
		{
			if (value == null)
			{
				NHibernateUtil.AnsiString.NullSafeSet(cmd, null, index);
			}
			else
			{
				NHibernateUtil.AnsiString.NullSafeSet(cmd, JsonConvert.SerializeObject(value), index);
			}
		}

		public object Replace(object original, object target, object owner)
		{
			return original;
		}

		public Type ReturnedType
		{
			get { return typeof(object); }
		}

		public SqlType[] SqlTypes
		{
			get { return new[] { new StringClobSqlType() }; }
		}

		new public bool Equals(object x, object y)
		{
			if (x == null && y == null)
				return true;

			if (x == null || y == null)
				return false;

			if (!x.Equals(y))
				return false;

			return true;
		}

		public int GetHashCode(object x)
		{
			if (x == null) return 0;
			return x.GetHashCode();
		}
	}
}