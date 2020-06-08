using System.Linq;
using Lucile.AspNetCore.ClientSettings;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class LucileAspNetCoreClientSettingsBuilderExtensions
    {
        public static IClientSettingsBuilder WithOptions<TOption>(this IClientSettingsBuilder builder)
        {
            RemoveSettingsService(builder);

            builder.Services.AddSingleton<IClientSettingsService, OptionSettingsService<TOption>>();

            return builder;
        }

        public static IClientSettingsBuilder WithConfiguration(this IClientSettingsBuilder builder, IConfigurationSection section)
        {
            RemoveSettingsService(builder);

            builder.Services.AddSingleton<IClientSettingsService>(new ConfigurationSectionSettingsService(section));
            return builder;
        }

        private static void RemoveSettingsService(IClientSettingsBuilder builder)
        {
            ServiceDescriptor toRemove = builder.Services.FirstOrDefault(p => p.ServiceType == typeof(IClientSettingsService));
            if (toRemove != null)
            {
                builder.Services.Remove(toRemove);
            }
        }
    }
}
