using System;
using System.Collections.Generic;
using System.Reflection;
using Lucile.Configuration.Plugin;
using Lucile.Core.Test.Module1;
using Lucile.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tests;
using Xunit;

[assembly: ServiceConfiguration(typeof(Startup))]
namespace Tests
{
    public class ServiceConfigurationTest
    {
        [Fact]
        public void AddServiceConfgiurationWithConfigConstructor()
        {
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddInMemoryCollection(new Dictionary<string, string>
            {
                { "test", "abc"}
            });

            var config = configBuilder.Build();

            var collection = new ServiceCollection();
            var options = new PluginOptions
            {
                Assemblies = {
                    typeof(ConnectedServiceTest).Assembly.GetName().Name,
                    typeof(Bootstrap).Assembly.GetName().Name,
                }
            };

            collection.AddPlugins(options, config);

            using (var serviceProvider = collection.BuildServiceProvider())
            {
                var dummy = serviceProvider.GetRequiredService<Dummy>();
                Assert.Equal("abc", dummy.Value);

                Assert.NotNull(serviceProvider.GetRequiredService<ITestService>());
            }
        }

        [Fact]
        public void AddServiceConfgiurationWithEmptyConfiguration()
        {
            var collection = new ServiceCollection();
            var options = new PluginOptions { Assemblies = { typeof(ConnectedServiceTest).Assembly.GetName().Name } };
            collection.AddPlugins(options);

            using (var serviceProvider = collection.BuildServiceProvider())
            {
                var dummy = serviceProvider.GetRequiredService<Dummy>();
                Assert.Null(dummy.Value);
            }
        }
    }

    public class Startup : IServiceConfiguration
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void Configure(IServiceCollection services)
        {
            services.AddSingleton<Dummy>(new Dummy(_configuration?.GetSection("test")?.Value));
        }
    }

    public class Dummy
    {
        public Dummy(string value)
        {
            Value = value;
        }

        public string Value { get; }
    }
}