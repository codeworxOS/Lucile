using Microsoft.Extensions.DependencyInjection;

namespace Lucile.Extensions.DependencyInjection
{
    public interface IServiceConfiguration
    {
        void Configure(IServiceCollection services);
    }
}