using System;

namespace Archon.Webhooks
{
	public class Event
	{
		public int Id { get; internal set; }
		public Webhook Hook { get; private set; }

		public string Type { get; private set; }
		public object Payload { get; private set; }

		public DateTime CreatedOn { get; private set; }
		public DateTime? LastAttempt { get; private set; }

		public string ErrorMessage { get; private set; }

		private Event() { }

		public Event(Webhook hook, string type, object payload)
		{
			if (hook == null)
				throw new ArgumentNullException("hook");

			if (String.IsNullOrWhiteSpace(type))
				throw new ArgumentNullException("type");

			if (payload == null)
				throw new ArgumentNullException("payload");

			this.Hook = hook;
			this.Type = type;
			this.Payload = payload;

			this.CreatedOn = DateTime.Now;
		}

		public void MarkAttempt(string errorMessage)
		{
			this.LastAttempt = DateTime.Now;
			this.ErrorMessage = errorMessage;
		}

		public override string ToString()
		{
			return Type;
		}
	}
}