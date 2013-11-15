using System;
using Nancy;
using Nancy.Responses;
using SimpleAuthentication.Core;
using System.Reactive.Linq;
using Akavache;
using System.Threading.Tasks;
using GitHub.Helpers;

namespace Peasant
{
    public class IndexModule : NancyModule
    {
        public IndexModule()
        {
            Get["/", runAsync: true] = async(x, ct) => {
                var ret = ensureAuthenticated(Context);
                if (ret != null) return ret;

                return View["index"];
            };

            Get["/post-receive", runAsync: true] = async (x, ct) => {
                return 201;
            };
        }

        dynamic ensureAuthenticated(NancyContext ctx)
        {
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
        }
    }
}