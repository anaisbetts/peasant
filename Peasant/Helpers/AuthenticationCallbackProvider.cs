using Akavache;
using Nancy;
using Nancy.SimpleAuthentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Reactive.Linq;
using System.Text;
using Nancy.Responses;

namespace Peasant.Helpers
{
    public class AuthenticationCallbackProvider : IAuthenticationCallbackProvider
    {
        public dynamic Process(NancyModule nancyModule, AuthenticateCallbackData model)
        {
            var sessionKey = "Session_" + model.AuthenticatedClient.AccessToken.PublicToken;
            BlobCache.Secure.InsertObject(sessionKey, model.AuthenticatedClient, TimeSpan.FromDays(1)).First();

            BlobCache.Secure.GetOrFetchObject("build-user", () => Observable.Return(model.AuthenticatedClient));

            nancyModule.Session["User"] = sessionKey;
            return new RedirectResponse("/");
        }

        public dynamic OnRedirectToAuthenticationProviderError(NancyModule nancyModule, string errorMessage)
        {
            return nancyModule.View["error"];
        }
    }
}