namespace Peasant
{
    using Akavache;
    using Nancy;
    using Nancy.Session;
    using Nancy.SimpleAuthentication;
    using Peasant.Helpers;
    using Peasant.Models;
    using SimpleAuthentication.Core;
    using SimpleAuthentication.ExtraProviders;
    using System;

    public class Bootstrapper : DefaultNancyBootstrapper
    {
        protected override void ApplicationStartup(Nancy.TinyIoc.TinyIoCContainer container, Nancy.Bootstrapper.IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);
            CookieBasedSessions.Enable(pipelines);
        }

        // The bootstrapper enables you to reconfigure the composition of the framework,
        // by overriding the various methods and properties.
        // For more information https://github.com/NancyFx/Nancy/wiki/Bootstrapper
        protected override void ConfigureRequestContainer(Nancy.TinyIoc.TinyIoCContainer container, NancyContext context)
        {
            base.ConfigureRequestContainer(container, context);

            var githubAuth = new GitHubProvider(new ProviderParams() {
                PublicApiKey = Environment.GetEnvironmentVariable("PEASANT_OAUTH_ID") ?? "68e704a8ede918f8d940",
                SecretApiKey = Environment.GetEnvironmentVariable("PEASANT_OAUTH_KEY") ?? "888b305d22b2d7251b087c5903be21f7eaca4e20",
                Scopes = new[] { "repo", "user", "status", }
            });

            var authServiceFactory = new AuthenticationProviderFactory();
            authServiceFactory.AddProvider(githubAuth);
            
            container.Register<IAuthenticationCallbackProvider>(new AuthenticationCallbackProvider());

            BlobCache.ApplicationName = "Peasant";
        }
    }
}