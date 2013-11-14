using System;
using Nancy;
using Nancy.Responses;
using SimpleAuthentication.Core;
using System.Reactive.Linq;
using Akavache;

namespace Peasant
{
    public class IndexModule : NancyModule
    {
        public IndexModule()
        {
            Get["/"] = parameters => {
                return View["index"];
            };

            Before.AddItemToEndOfPipeline(ctx => {
                if (ctx.Request.Session == null) {
                    goto fail;
                }

                var sessionKey = (ctx.Request.Session["User"] ?? "").ToString(); ;
                if (String.IsNullOrWhiteSpace(sessionKey)) {
                    goto fail;
                }

                try {
                    var user = BlobCache.Secure.GetObjectAsync<AuthenticatedClient>(sessionKey).First();
                    if (user == null) goto fail;
                } catch (Exception ex) {
                    goto fail;
                }

                return null;

            fail:
                return new RedirectResponse("/authentication/redirect/github");
            });
        }
    }
}