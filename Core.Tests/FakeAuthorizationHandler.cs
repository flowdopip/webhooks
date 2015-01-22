using System;
using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace Archon.Webhooks.Tests
{
	public enum AuthStatus { Authorized, AuthenticatedWithoutPermissions, NotAuthenticated }

	public class FakeAuthorizationHandler : DelegatingHandler
	{
		string username;
		AuthStatus auth;

		public void Authorize(string username)
		{
			this.username = username;
			this.auth = AuthStatus.Authorized;
		}

		public void AuthenticateWithoutPermissions(string username)
		{
			this.username = username;
			this.auth = AuthStatus.AuthenticatedWithoutPermissions;
		}

		public void ClearCredentials()
		{
			this.username = null;
			this.auth = AuthStatus.NotAuthenticated;
		}

		public FakeAuthorizationHandler()
		{
			this.username = "homer.simpson";
			this.auth = AuthStatus.Authorized;
		}

		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			switch (auth)
			{
				case AuthStatus.Authorized:
					request.GetRequestContext().Principal = new GenericPrincipal(new GenericIdentity(username), new[] { "Webhooks" });
					break;

				case AuthStatus.AuthenticatedWithoutPermissions:
					request.GetRequestContext().Principal = new GenericPrincipal(new GenericIdentity(username), new string[0]);
					break;

				case AuthStatus.NotAuthenticated:
					request.GetRequestContext().Principal = null;
					break;
			}

			return base.SendAsync(request, cancellationToken);
		}
	}
}