using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Lucile.ServiceModel.Test
{
    public class RemoteServiceProviderTest : IDisposable
    {
        private readonly ServiceHost _serviceHost;

        public RemoteServiceProviderTest()
        {
            _serviceHost = new ServiceHost(typeof(ServiceProviderService));
            NetTcpBinding binding = new NetTcpBinding(SecurityMode.None);
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.None;
            _serviceHost.AddServiceEndpoint(typeof(IServiceProviderService), binding, "net.tcp://localhost:4567/ServiceProviderService.svc");
            _serviceHost.AddServiceEndpoint(typeof(IServiceProviderService), binding, "net.tcp://localhost:4567/bla");

            _serviceHost.Open();
        }

        [Fact]
        public async Task AddressConvention()
        {
            var services = new ServiceCollection();
            services.AddConnectedService<IServiceProviderService>();
            services.UseRemoteServices(options =>
                options.Base("net.tcp://localhost:4567")
                .Credentials(ServiceAuthentication.None)
                .Addressing((ba, contract) => $"{ba}/bla.svc"));

            var provider = services.BuildServiceProvider(true);
            using (var scope = provider.CreateScope())
            {
                var client = scope.ServiceProvider.GetService<IServiceProviderService>();
                Assert.Equal(new Uri("net.tcp://localhost:4567/bla.svc"), ((IClientChannel)client).RemoteAddress.Uri);
            }
        }

        public void Dispose()
        {
            _serviceHost.Close();
        }

        [Fact]
        public async Task OnChannelFactoryAction()
        {
            var services = new ServiceCollection();
            services.AddConnectedService<IServiceProviderService>();
            services.UseRemoteServices(options =>
                options.Base("net.tcp://localhost:4567")
                .Credentials(ServiceAuthentication.None)
                .OnChannelFactory(cf => cf.Endpoint.EndpointBehaviors.Add(new TestHeaderEnpointBehavior())));

            var provider = services.BuildServiceProvider(true);
            using (var scope = provider.CreateScope())
            {
                var client = scope.ServiceProvider.GetService<IServiceProviderService>();
                var message = Guid.NewGuid().ToString("N");
                var fromService = await client.GetTestHeaderContent();
                Assert.Equal("TestContent", fromService);
            }
        }

        [Fact]
        public async Task SimpleRemoteServiceSetup()
        {
            var services = new ServiceCollection();
            services.AddConnectedService<IServiceProviderService>();
            services.UseRemoteServices(options =>
                options.Base("net.tcp://localhost:4567")
                .Credentials(ServiceAuthentication.None));

            var provider = services.BuildServiceProvider(true);
            using (var scope = provider.CreateScope())
            {
                var client = scope.ServiceProvider.GetService<IServiceProviderService>();
                var message = Guid.NewGuid().ToString("N");
                var fromService = await client.Hello(message);
                Assert.Equal(message, fromService);
            }
        }

        [Fact]
        public async Task ValidateScoping()
        {
            var services = new ServiceCollection();
            services.AddConnectedService<IServiceProviderService>();
            services.UseRemoteServices(options =>
                options.Base("net.tcp://localhost:4567")
                .Credentials(ServiceAuthentication.None)
                .OnChannelFactory(cf => cf.Endpoint.EndpointBehaviors.Add(new TestHeaderEnpointBehavior())));

            var provider = services.BuildServiceProvider(true);

            Assert.ThrowsAny<InvalidOperationException>(() => provider.GetService<IServiceProviderService>());
        }

        private class TestHeaderEnpointBehavior : IEndpointBehavior
        {
            public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
            {
            }

            public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
            {
                clientRuntime.ClientMessageInspectors.Add(new TestHeaderMessageInspector());
            }

            public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
            {
            }

            public void Validate(ServiceEndpoint endpoint)
            {
            }

            private class TestHeaderMessageInspector : IClientMessageInspector
            {
                public void AfterReceiveReply(ref Message reply, object correlationState)
                {
                }

                public object BeforeSendRequest(ref Message request, IClientChannel channel)
                {
                    request.Headers.Add(MessageHeader.CreateHeader("TestHeader", "TestNamespace", "TestContent"));
                    return null;
                }
            }
        }
    }
}