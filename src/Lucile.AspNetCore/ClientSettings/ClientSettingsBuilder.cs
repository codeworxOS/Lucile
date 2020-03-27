using Microsoft.Extensions.DependencyInjection;

namespace Lucile.AspNetCore.ClientSettings
{
    public class ClientSettingsBuilder : IClientSettingsBuilder
    {
        public ClientSettingsBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }
    }
}
