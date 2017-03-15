using System;
using Lucile.Service;
using Lucile.ServiceModel;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class LucileServiceModelServiceCollectionExtensions
    {
        public static IServiceCollection UseRemoteServices(this IServiceCollection serviceCollection, Action<RemoteServiceOptionsBuilder> optionBuilder)
        {
            return serviceCollection.AddSingleton<IConnectionFactory>(p =>
            {
                var builder = new RemoteServiceOptionsBuilder();
                optionBuilder(builder);
                return new RemoteConnectionFactory(builder.ToOptions());
            });
        }
    }
}