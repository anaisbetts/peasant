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
            var rng = new RNGCryptoServiceProvider();
            var bytes = new byte[512]; rng.GetNonZeroBytes(bytes);

            var sessionKey = "Session_" + BitConverter.ToString(bytes).Replace("-", "");
            BlobCache.Secure.InsertObject(sessionKey, model.AuthenticatedClient, TimeSpan.FromDays(1)).First();

            // XXX: This doesn't seem to work
            return new RedirectResponse("/");
        }

        public dynamic OnRedirectToAuthenticationProviderError(NancyModule nancyModule, string errorMessage)
        {
            return nancyModule.View["error"];
        }
    }
}