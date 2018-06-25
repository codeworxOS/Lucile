using Lucile.Dynamic.DependencyInjection.Service;
using Lucile.Service;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class LucileDynamicDependencyInjectionServiceCollectionExtensions
    {
        public static IServiceCollection AddScopeProxy(this IServiceCollection services)
        {
            return services.AddSingleton(typeof(IConnectedServiceProxy<>), typeof(ServiceScopeProxy<>));
        }
    }
}