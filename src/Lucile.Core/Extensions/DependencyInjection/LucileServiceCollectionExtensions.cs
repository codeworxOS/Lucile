using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Lucile.Extensions.DependencyInjection
{
    public static class LucileServiceCollectionExtensions
    {
        public static IServiceCollection FromConfiguration(this IServiceCollection collection, Assembly assembly)
        {
            var config = assembly.GetCustomAttribute<ServiceConfigurationAttribute>();
            if (config != null)
            {
                var conf = Activator.CreateInstance(config.ConfigurationType) as IServiceConfiguration;
                if (conf != null)
                {
                    conf.Configure(collection);
                }
            }

            return collection;
        }
    }
}