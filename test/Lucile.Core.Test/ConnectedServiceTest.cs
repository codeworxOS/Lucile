using System;
using System.Reflection;
using Lucile.Core.Test.Module1;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Tests
{
    public class ConnectedServiceTest
    {
        [Fact]
        public void AssemblyBootstrap()
        {
            var collection = new ServiceCollection();
            collection.FromConfiguration(typeof(ITestService).GetTypeInfo().Assembly);

            var provider = collection.BuildServiceProvider();
            var service = provider.GetService<ITestService>();

            Assert.IsType<TestService>(service);
        }

        [Fact]
        public void ConnectedServiceNullableInjection_ExpectsOK()
        {
            var collection = new ServiceCollection();

            collection.AddConnectedService<ITestService>();
            collection.AddScoped<SampleNullInjectionViewModel>();

            var provider = collection.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true }); ;

            using (var scope = provider.CreateScope())
            {
                var viewModel = scope.ServiceProvider.GetRequiredService<SampleNullInjectionViewModel>();

                Assert.Null(viewModel.Service);
            }
        }

        [Fact]
        public void ConnectedServiceNoneNullableInjection_ExpectsError()
        {
            var collection = new ServiceCollection();

            collection.AddConnectedService<ITestService>();
            collection.AddScoped<SampleNoneNullInjectionViewModel>();

            var provider = collection.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true }); ;

            using (var scope = provider.CreateScope())
            {
                var ex = Assert.Throws<ArgumentNullException>(() =>
                {
                    var viewModel = scope.ServiceProvider.GetRequiredService<SampleNoneNullInjectionViewModel>();
                });

                Assert.Equal("service", ex.ParamName);
            }
        }

        private class SampleNullInjectionViewModel
        {
            public SampleNullInjectionViewModel(ITestService service = null)
            {
                Service = service;
            }

            public ITestService Service { get; }
        }

        private class SampleNoneNullInjectionViewModel
        {
            public SampleNoneNullInjectionViewModel(ITestService service)
            {
                Service = service ?? throw new System.ArgumentNullException(nameof(service));
            }

            public ITestService Service { get; }
        }
    }
}