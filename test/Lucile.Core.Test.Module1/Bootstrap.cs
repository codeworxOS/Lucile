using Lucile.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: ServiceConfiguration(typeof(Lucile.Core.Test.Module1.Bootstrap))]

namespace Lucile.Core.Test.Module1
{
    public class Bootstrap : IServiceConfiguration
    {
        public void Configure(IServiceCollection services)
        {
            services.AddConnectedService<ITestService>();
            services.AddConnected<ITestService, TestService>();
        }
    }
}