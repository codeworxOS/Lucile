using Lucile.AspNetCore.ClientSettings;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNetCore.Builder
{
    public static class LucileAspNetCoreApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseClientSettings(this IApplicationBuilder app, string path)
        {
            app.Map(path, p => p.UseMiddleware<ClientSettingsMiddleware>());

            return app;
        }

        public static IEndpointConventionBuilder MapClientSettings(this IEndpointRouteBuilder endpoint, string path)
        {
            var pipeline = endpoint.CreateApplicationBuilder()
                  .UseMiddleware<ClientSettingsMiddleware>()
                  .Build();

            return endpoint.Map(path, pipeline)
                        .WithDisplayName("Client Settings");
        }
    }
}
