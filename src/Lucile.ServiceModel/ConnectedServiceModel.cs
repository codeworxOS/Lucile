using System;
using System.ServiceModel;
using Lucile.Service;

namespace Lucile.ServiceModel
{
    internal class ConnectedServiceModel<TService> : IConnected<TService>
    {
        private readonly IServiceProvider _serviceProvider;
        private RemoteServiceOptions _remoteServiceOptions;

        public ConnectedServiceModel(IServiceProvider serviceProvider, RemoteServiceOptions remoteServiceOptions)
        {
            _serviceProvider = serviceProvider;
            _remoteServiceOptions = remoteServiceOptions;
        }

        public TService GetService()
        {
            var binding = _remoteServiceOptions.GetBinding<TService>();
            var endpointAddress = _remoteServiceOptions.GetEndpointAddress<TService>();
            var cf = new ChannelFactory<TService>(binding, endpointAddress);
            _remoteServiceOptions?.OnChannelFactoryAction?.Invoke(_serviceProvider, cf);
            return cf.CreateChannel();
        }
    }
}