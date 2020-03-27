using Lucile.AspNetCore.ClientSettings;

#if NETCOREAPP3_1
using Microsoft.AspNetCore.Routing;
#endif

namespace Microsoft.AspNetCore.Builder
{
    public static class LucileAspNetCoreApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseClientSettings(this IApplicationBuilder app, string path)
        {
            app.Map(path, p => p.UseMiddleware<ClientSettingsMiddleware>());

            return app;
        }

#if NETCOREAPP3_1
        public static IEndpointConventionBuilder MapClientSettings(this IEndpointRouteBuilder endpoint, string path)
        {
            var pipeline = endpoint.CreateApplicationBuilder()
                  .UseMiddleware<ClientSettingsMiddleware>()
                  .Build();

            return endpoint.Map(path, pipeline)
                        .WithDisplayName("Client Settings");
        }
#endif
    }
}
