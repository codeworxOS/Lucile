using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using Lucile.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Lucile.ServiceModel.DependencyInjection.Behavior
{
    public class ServiceProviderBehavior : IServiceBehavior
    {
        public ServiceProviderBehavior()
        {
        }

        public ServiceProviderBehavior(IServiceProvider serviceProvider)
        {
            this.ServiceProvider = serviceProvider;
        }

        public ServiceProviderBehavior(Type serviceConfiguration)
        {
            this.ServiceConfiguration = serviceConfiguration;
        }

        public Type ServiceConfiguration { get; set; }

        public IServiceProvider ServiceProvider { get; }

        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            foreach (var dispatcher in serviceHostBase.ChannelDispatchers)
            {
                var channelDispatcher = dispatcher as ChannelDispatcher;

                if (channelDispatcher == null)
                {
                    continue;
                }

                foreach (EndpointDispatcher endpointDispatcher in channelDispatcher.Endpoints)
                {
                    var provider = GetServiceProvider();

                    endpointDispatcher.DispatchRuntime.MessageInspectors.Add(new ServiceProviderMessageInspector(provider));
                    endpointDispatcher.DispatchRuntime.InstanceProvider = new ServiceInstanceProvider(provider);
                }
            }
        }

        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
        }

        private IServiceProvider GetServiceProvider()
        {
            if (ServiceProvider != null)
            {
                return ServiceProvider;
            }

            var collection = new ServiceCollection();
            var configuration = Activator.CreateInstance(ServiceConfiguration) as IServiceConfiguration;

            if (configuration == null)
            {
                throw new NotSupportedException($"ServiceConfigurationType is missing or does not implement IServiceConfiguration!");
            }

            configuration.Configure(collection);

            return collection.BuildServiceProvider();
        }
    }
}