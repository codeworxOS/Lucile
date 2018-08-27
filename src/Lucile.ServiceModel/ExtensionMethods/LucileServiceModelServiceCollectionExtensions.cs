using System;
using Lucile.Service;
using Lucile.ServiceModel;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class LucileServiceModelServiceCollectionExtensions
    {
        public static IServiceCollection UseRemoteServices(this IServiceCollection serviceCollection, Action<RemoteServiceOptionsBuilder> optionBuilder)
        {
            serviceCollection.AddSingleton<RemoteServiceOptions>(sp =>
            {
                var builder = new RemoteServiceOptionsBuilder();
                optionBuilder(builder);
                return builder.ToOptions();
            });

            return serviceCollection.AddSingleton<IConnectionFactory, RemoteConnectionFactory>();
        }
    }
}