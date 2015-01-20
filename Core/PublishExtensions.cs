﻿using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Archon.Webhooks
{
	public static class PublishExtensions
	{
		public static async Task<PublishResult> PublishEvent(this HttpClient client, Event evt)
		{
			HttpResponseMessage response = null;
			Exception httpEx = null;

			try
			{
				response = await client.PostAsJsonAsync(evt.Hook.Url, new
				{
					evt.Id,
					evt.Type,
					evt.Payload
				});
			}
			catch (Exception ex)
			{
				httpEx = ex;
			}

			if (response != null && response.IsSuccessStatusCode)
				return new PublishResult(true, String.Format("Successfully POSTed event with ID '{0}', type '{1}' to {2}", evt.Id, evt.Type, evt.Hook.Url));

			string message = String.Format("Error POSTing event with ID '{1}', type '{2}' to {3}.{0}Event: {4}",
				Environment.NewLine,
				evt.Id,
				evt.Type,
				evt.Hook.Url,
				JsonConvert.SerializeObject(evt.Payload)
			);

			if (response != null && response.Content != null)
			{
				string content = await response.Content.ReadAsStringAsync();
				message += Environment.NewLine + String.Format("Response: {0}, {1}", response.StatusCode, content);
			}

			if (httpEx != null)
			{
				message += Environment.NewLine + String.Format("Exception: {0}", httpEx);
			}

			evt.MarkAttempt(message);
			return new PublishResult(false, message);
		}
	}
}