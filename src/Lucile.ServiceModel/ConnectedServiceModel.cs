using System;
using System.ServiceModel;
using Lucile.Service;
using Microsoft.Extensions.DependencyInjection;

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
            var binding = _remoteServiceOptions.GetBinding<TService>(out var callback);
            var endpointAddress = _remoteServiceOptions.GetEndpointAddress<TService>();
            ChannelFactory<TService> cf = null;
            if (callback == null)
            {
                cf = new ChannelFactory<TService>(binding, endpointAddress);
            }
            else
            {
#if NETSTANDARD1_3
                throw new NotSupportedException($"Duplex Services are not supported on net standard");
#else
                var instance = _serviceProvider.GetRequiredService(callback);
                cf = new DuplexChannelFactory<TService>(instance, binding, endpointAddress);
#endif
            }

            _remoteServiceOptions?.OnChannelFactoryAction?.Invoke(_serviceProvider, cf);
            return cf.CreateChannel();
        }
    }
}