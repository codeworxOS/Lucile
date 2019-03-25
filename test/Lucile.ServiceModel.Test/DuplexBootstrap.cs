using System.ServiceModel;
using Lucile.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Lucile.ServiceModel.Test
{
    public class DuplexBootstrap : IServiceConfiguration
    {
        public void Configure(IServiceCollection services)
        {
            services.AddScoped<IDuplexCallback>(sp => OperationContext.Current.GetCallbackChannel<IDuplexCallback>());
            services.AddScoped<DuplexServiceProviderService>();
        }
    }
}