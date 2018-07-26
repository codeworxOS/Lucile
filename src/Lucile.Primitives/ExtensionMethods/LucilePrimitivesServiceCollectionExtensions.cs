using System;
using System.Linq.Expressions;
using System.Reflection;
using Lucile.Service;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class LucilePrimitivesServiceCollectionExtensions
    {
        private static MethodInfo _getServiceImplementationMethod;

        static LucilePrimitivesServiceCollectionExtensions()
        {
            Expression<Func<IServiceProvider, object>> getServiceImplementationExpression = p => GetServiceImplementation<object>(p);
            _getServiceImplementationMethod = ((MethodCallExpression)getServiceImplementationExpression.Body).Method.GetGenericMethodDefinition();
        }

        [Obsolete("Use AddConnectedLocal instead.", true)]
        public static IServiceCollection AddConnected<TService, TImplementation>(this IServiceCollection serviceCollection)
            where TService : class
            where TImplementation : class, TService
        {
            return serviceCollection
                        .AddScoped<IConnected<TService>, ConnectedInstance<TService, TImplementation>>()
                        .AddScoped<TImplementation>();
        }

        public static IServiceCollection AddConnectedLocal<TService, TImplementation>(this IServiceCollection serviceCollection)
            where TService : class
            where TImplementation : class, TService
        {
            return serviceCollection
                .AddScoped<TImplementation>()
                .AddScoped<IConnected<TService>, ConnectedInstance<TService, TImplementation>>();
        }

        public static IServiceCollection AddConnectedService(this IServiceCollection serviceCollection, Type serviceType)
        {
            var param = Expression.Parameter(typeof(IServiceProvider));
            var func = Expression.Lambda<Func<IServiceProvider, object>>(Expression.Call(_getServiceImplementationMethod.MakeGenericMethod(serviceType), param), param).Compile();
            return serviceCollection.AddScoped(serviceType, func);
        }

        public static IServiceCollection AddConnectedService<TService>(this IServiceCollection serviceCollection)
            where TService : class
        {
            return serviceCollection.AddScoped<TService>(GetServiceImplementation<TService>);
        }

        [Obsolete("Not needed any more.")]
        public static IServiceCollection AddLocalService<TService, TImplementation>(this IServiceCollection serviceCollection)
            where TService : class
            where TImplementation : class, TService
        {
            return serviceCollection
                .AddScoped<TImplementation>()
                .AddScoped<ILocal<TService>, LocalServiceImplementation<TService, TImplementation>>();
        }

        public static TService GetConnectedService<TService>(this IServiceProvider serviceProvider)
            where TService : class
        {
            var connected = serviceProvider.GetService<IConnected<TService>>();

            if (connected == null)
            {
                var factory = serviceProvider.GetService<IConnectionFactory>();

                if (factory == null)
                {
                    throw new MissingConnectionFactoryException();
                }

                connected = factory.GetConnectedService<TService>(serviceProvider);
            }

            return connected?.GetService();
        }

        [Obsolete("No longer needed")]
        public static IServiceCollection UseLocalServices(this IServiceCollection serviceCollection)
        {
            return serviceCollection.AddSingleton<IConnectionFactory, LocalConnectionFactory>();
        }

        private static TService GetServiceImplementation<TService>(IServiceProvider serviceProvider)
            where TService : class
        {
            var proxy = serviceProvider.GetService<IConnectedServiceProxy<TService>>();
            return proxy?.GetProxy(serviceProvider) ?? serviceProvider.GetConnectedService<TService>();
        }
    }
}