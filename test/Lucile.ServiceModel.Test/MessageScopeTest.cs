using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Threading.Tasks;
using Lucile;
using Lucile.ServiceModel.Behavior;
using Lucile.ServiceModel.DependencyInjection.Behavior;
using Lucile.ServiceModel.Test;
using Xunit;

namespace Tests
{
    public class MessageScopeTest
    {
        [Fact]
        public async Task MessageScopeTestAsync()
        {
            var path = Guid.NewGuid().ToString();
            using (var sh = new ServiceHost(typeof(MessageScopeService), new Uri($"net.tcp://localhost:1234/{path}")))
            {
                sh.AddServiceEndpoint(typeof(IMessageScopeService), new NetTcpBinding(SecurityMode.None), string.Empty);
                sh.Description.Behaviors.Add(new ServiceProviderBehavior(typeof(BootstrapMessageScope)));

                sh.Open();

                using (var cf = new ChannelFactory<IMessageScopeService>(new NetTcpBinding(SecurityMode.None), new EndpointAddress($"net.tcp://localhost:1234/{path}")))
                {
                    var channel = cf.CreateChannel();

                    var result = await channel.TestAsync();

                    Assert.Equal($"net.tcp://localhost:1234/{path}", result);
                }
            }
        }
    }
}