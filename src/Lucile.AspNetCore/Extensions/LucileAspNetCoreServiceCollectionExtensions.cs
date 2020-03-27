using Lucile.AspNetCore.ClientSettings;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class LucileAspNetCoreServiceCollectionExtensions
    {
        public static IClientSettingsBuilder AddClientSettings(this IServiceCollection services)
        {
            return new ClientSettingsBuilder(services);
        }
    }
}
