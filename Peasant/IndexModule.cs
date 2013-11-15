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
                var ret = await ensureAuthenticated(Context);
                if (ret != null) return ret;

                return View["index"];
            };

            Get["/post-receive", runAsync: true] = async (x, ct) => {
                // 1. Grab out params
                // 1. Determine clone target directory
                // 1. Do the clone / fetch
                // 1. Do a hard reset + clean -xdf
                // 1. Download build script to target directory
                // 1. Set up environment variables, kick off build script

                return 201;
            };
        }

        async Task<dynamic> ensureAuthenticated(NancyContext ctx)
        {
            if (ctx.Request.Session == null) {
                goto fail;
            }

            var sessionKey = (ctx.Request.Session["User"] ?? "").ToString(); ;
            if (String.IsNullOrWhiteSpace(sessionKey)) {
                goto fail;
            }

            try {
                var user = await BlobCache.Secure.GetObjectAsync<AuthenticatedClient>(sessionKey);
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