using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Threading.Tasks;
using Lucile;
using Lucile.ServiceModel.Behavior;
using Lucile.ServiceModel.Test;
using Xunit;

namespace Tests
{
    public class ErrorHandlerTest
    {
        [Fact]
        private async Task IncludeDetailsFalseAsync()
        {
            var path = Guid.NewGuid().ToString();
            using (var sh = new ServiceHost(typeof(ErrorService), new Uri($"net.tcp://localhost:1234/{path}")))
            {
                var ep = sh.AddServiceEndpoint(typeof(IErrorService), new NetTcpBinding(SecurityMode.None), string.Empty);
                ep.EndpointBehaviors.Add(new Lucile.ServiceModel.Behavior.ErrorHandlerEndpointBehavior() { IncludeDetails = false, LogDelegate = p => "LogIdentifier" });

                sh.Open();

                using (var cf = new ChannelFactory<IErrorService>(new NetTcpBinding(SecurityMode.None), new EndpointAddress($"net.tcp://localhost:1234/{path}")))
                {
                    cf.Endpoint.EndpointBehaviors.Add(new ErrorHandlerEndpointBehavior());
                    var channel = cf.CreateChannel();

                    var error = Assert.Throws<ServiceProcessingException>(() => channel.RaiseError());
                    Assert.True(error.Message.StartsWith("Service request threw exception System.ArgumentException"));
                    Assert.Equal("LogIdentifier", error.TracingIdentifier);

                    error = await Assert.ThrowsAsync<ServiceProcessingException>(async () => await channel.RaiseAsyncErrorAsync());
                    Assert.True(error.Message.StartsWith("Service request threw exception System.ArgumentException"));
                    Assert.Equal("LogIdentifier", error.TracingIdentifier);
                }
            }
        }

        [Fact]
        private async Task IncludeDetailsTrueAsync()
        {
            var path = Guid.NewGuid().ToString();
            using (var sh = new ServiceHost(typeof(ErrorService), new Uri($"net.tcp://localhost:1234/{path}")))
            {
                var ep = sh.AddServiceEndpoint(typeof(IErrorService), new NetTcpBinding(SecurityMode.None), string.Empty);
                ep.EndpointBehaviors.Add(new Lucile.ServiceModel.Behavior.ErrorHandlerEndpointBehavior() { IncludeDetails = true });

                sh.Open();

                using (var cf = new ChannelFactory<IErrorService>(new NetTcpBinding(SecurityMode.None), new EndpointAddress($"net.tcp://localhost:1234/{path}")))
                {
                    cf.Endpoint.EndpointBehaviors.Add(new ErrorHandlerEndpointBehavior());
                    var channel = cf.CreateChannel();

                    var error = Assert.Throws<ArgumentException>(() => channel.RaiseError());
                    Assert.Equal("Whatever", error.Message);

                    error = await Assert.ThrowsAsync<ArgumentException>(async () => await channel.RaiseAsyncErrorAsync());
                    Assert.Equal("Whatever", error.Message);
                }
            }
        }

        [Fact]
        private async Task WebHttpBindingIncludeDetailsFalseAsync()
        {
            var path = Guid.NewGuid().ToString();
            using (var sh = new ServiceHost(typeof(ErrorService), new Uri($"http://localhost:4512/{path}")))
            {
                var ep = sh.AddServiceEndpoint(typeof(IErrorService), new WebHttpBinding(), string.Empty);
                ep.EndpointBehaviors.Add(new WebHttpBehavior { FaultExceptionEnabled = true });
                ep.EndpointBehaviors.Add(new Lucile.ServiceModel.Behavior.ErrorHandlerEndpointBehavior() { IncludeDetails = true, LogDelegate = p => "LogIdentifier" });

                sh.Open();

                using (var cf = new ChannelFactory<IErrorService>(new WebHttpBinding(), new EndpointAddress($"http://localhost:4512/{path}")))
                {
                    cf.Endpoint.EndpointBehaviors.Add(new ErrorHandlerEndpointBehavior());
                    cf.Endpoint.EndpointBehaviors.Add(new WebHttpBehavior());
                    var channel = cf.CreateChannel();

                    var error = Assert.Throws<ArgumentException>(() => channel.RaiseError());
                    Assert.Equal("Whatever", error.Message);

                    error = await Assert.ThrowsAsync<ArgumentException>(async () => await channel.RaiseAsyncErrorAsync());
                    Assert.Equal("Whatever", error.Message);
                }
            }
        }

        [Fact]
        private async Task WebHttpBindingIncludeDetailsTrueAsync()
        {
            var path = Guid.NewGuid().ToString();
            using (var sh = new ServiceHost(typeof(ErrorService), new Uri($"http://localhost:4512/{path}")))
            {
                var ep = sh.AddServiceEndpoint(typeof(IErrorService), new WebHttpBinding(), string.Empty);
                ep.EndpointBehaviors.Add(new WebHttpBehavior { FaultExceptionEnabled = true });
                ep.EndpointBehaviors.Add(new Lucile.ServiceModel.Behavior.ErrorHandlerEndpointBehavior() { IncludeDetails = false, LogDelegate = p => "LogIdentifier" });

                sh.Open();

                using (var cf = new ChannelFactory<IErrorService>(new WebHttpBinding(), new EndpointAddress($"http://localhost:4512/{path}")))
                {
                    cf.Endpoint.EndpointBehaviors.Add(new ErrorHandlerEndpointBehavior());
                    cf.Endpoint.EndpointBehaviors.Add(new WebHttpBehavior());
                    var channel = cf.CreateChannel();

                    var error = Assert.Throws<ServiceProcessingException>(() => channel.RaiseError());
                    Assert.True(error.Message.StartsWith("Service request threw exception System.ArgumentException"));
                    Assert.Equal("LogIdentifier", error.TracingIdentifier);

                    error = await Assert.ThrowsAsync<ServiceProcessingException>(async () => await channel.RaiseAsyncErrorAsync());
                    Assert.True(error.Message.StartsWith("Service request threw exception System.ArgumentException"));
                    Assert.Equal("LogIdentifier", error.TracingIdentifier);
                }
            }
        }
    }
}