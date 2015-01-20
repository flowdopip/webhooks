using System;

namespace Archon.Webhooks
{
	public class PublishResult
	{
		public bool IsSuccess { get; private set; }
		public string Message { get; private set; }

		public PublishResult(bool isSuccess, string message)
		{
			this.IsSuccess = isSuccess;
			this.Message = message;
		}

		public override string ToString()
		{
			return IsSuccess ? "Success" : "Failure";
		}
	}
}