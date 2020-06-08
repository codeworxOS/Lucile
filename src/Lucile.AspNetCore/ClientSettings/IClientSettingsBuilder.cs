using Microsoft.Extensions.DependencyInjection;

namespace Lucile.AspNetCore.ClientSettings
{
    public interface IClientSettingsBuilder
    {
        IServiceCollection Services { get; }
    }
}
