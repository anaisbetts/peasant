using System;
using Nancy;
using Nancy.Responses;
using SimpleAuthentication.Core;
using System.Reactive.Linq;
using Akavache;
using System.Threading.Tasks;
using GitHub.Helpers;
using Octokit;
using System.Net.Http.Headers;

namespace Peasant
{
    public class ReturnTheDamnCredentialsStore : ICredentialStore
    {
        readonly Credentials creds;
        public ReturnTheDamnCredentialsStore(Credentials creds)
        {
            this.creds = creds;
        }

        public Task<Credentials> GetCredentials()
        {
            return Task.FromResult(creds);
        }
    }

    public class IndexModule : NancyModule
    {
        public IndexModule()
        {
            Get["/", runAsync: true] = async(x, ct) => {
                var user = await ensureAuthenticated(Context);
                if (user == null) return new RedirectResponse("/authentication/redirect/github");

                var client = new GitHubClient(
                    new ProductHeaderValue("Peasant", "0.0.1"), 
                    new ReturnTheDamnCredentialsStore(new Credentials(user.UserInformation.UserName, user.AccessToken.PublicToken)));

                var orgs = await client.Organization.GetAllForCurrent();

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

        async Task<AuthenticatedClient> ensureAuthenticated(NancyContext ctx)
        {
            if (ctx.Request.Session == null) {
                goto fail;
            }

            var sessionKey = (ctx.Request.Session["User"] ?? "").ToString(); ;
            if (String.IsNullOrWhiteSpace(sessionKey)) {
                goto fail;
            }

            var user = default(AuthenticatedClient);
            try {
                user = await BlobCache.Secure.GetObjectAsync<AuthenticatedClient>(sessionKey);
                if (user == null) goto fail;
            } catch (Exception ex) {
                goto fail;
            }

            return user;

        fail:
            return null;
        }
    }
}