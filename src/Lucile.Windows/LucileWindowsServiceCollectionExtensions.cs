using System;
using System.Windows.Threading;
using Lucile;
using Lucile.Threading;
using Lucile.Windows;
using Lucile.Windows.Threading;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class LucileWindowsServiceCollectionExtensions
    {
        public static IServiceCollection AddLucilePresentation(this IServiceCollection services, Dispatcher dispatcher)
        {
            return services
                    .AddSingleton(dispatcher)
                    .AddSingleton<ICollectionSynchronization, CollectionSynchronization>()
                    .AddSingleton<IViewOperations, ViewOperations>();
        }

        public static IServiceCollection AddLucilePresentation(this IServiceCollection services, Func<IServiceProvider, Dispatcher> dispatcher)
        {
            return services
                    .AddSingleton(dispatcher)
                    .AddSingleton<ICollectionSynchronization, CollectionSynchronization>()
                    .AddSingleton<IViewOperations, ViewOperations>();
        }
    }
}