using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading.Tasks;
using Lucile.ServiceModel.DependencyInjection.Behavior;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Lucile.ServiceModel.Test
{
    public class DuplexServiceProviderTest : IDisposable
    {
        private readonly ServiceHost _serviceHost;

        public DuplexServiceProviderTest()
        {
            _serviceHost = new ServiceHost(typeof(DuplexServiceProviderService));
            NetTcpBinding binding = new NetTcpBinding(SecurityMode.None);
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.None;
            _serviceHost.AddServiceEndpoint(typeof(IDuplexServiceProviderService), binding, "net.tcp://localhost:5678/DuplexServiceProviderService.svc");
            _serviceHost.Description.Behaviors.Add(new ServiceProviderBehavior(typeof(DuplexBootstrap)));

            _serviceHost.Open();
        }

        public void Dispose()
        {
            _serviceHost.Close();
        }

        [Fact]
        public async Task DuplexStaticCallbackChannelTest()
        {
            int progressCalled = 0, finishedCalled = 0;
            var tcs = new TaskCompletionSource<bool>();

            var services = new ServiceCollection();
            services.AddConnectedService<IDuplexServiceProviderService>();
            services.AddScoped<IDuplexCallback>(sp =>
            {
                var result = new TestDuplexCallback();
                result.Progress += p => progressCalled++;
                result.Finished += () =>
                {
                    finishedCalled++;
                    tcs.SetResult(true);
                };

                return result;
            });
            services
                .AddScopeProxy()
                .UseRemoteServices(options =>
                options.Base("net.tcp://localhost:5678")
                .Credentials(ServiceAuthentication.None));

            var provider = services.BuildServiceProvider(true);
            using (var scope = provider.CreateScope())
            {
                var client = scope.ServiceProvider.GetService<IDuplexServiceProviderService>();
                await client.StartImportAsync();

                var called = await tcs.Task;
                Assert.True(called);
                Assert.Equal(10, progressCalled);
                Assert.Equal(1, finishedCalled);
            }
        }

        private class TestDuplexCallback : IDuplexCallback
        {
            public event Action Finished;

            public event Action<decimal> Progress;

            public void OnFinished()
            {
                Finished?.Invoke();
            }

            public void OnProgress(decimal progress)
            {
                Progress?.Invoke(progress);
            }
        }
    }
}