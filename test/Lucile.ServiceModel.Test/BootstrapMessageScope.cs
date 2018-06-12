using Lucile.Extensions.DependencyInjection;
using Lucile.ServiceModel.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Tests
{
    internal class BootstrapMessageScope : IServiceConfiguration
    {
        public void Configure(IServiceCollection services)
        {
            services.AddTransient<IMessageScopeInitializer, SampleInfoInitializer>();
            services.AddScoped<SampleInfo>();

            services.AddTransient<MessageScopeService>();
        }
    }
}