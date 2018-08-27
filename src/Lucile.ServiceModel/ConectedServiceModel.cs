using System.ServiceModel;
using Lucile.Service;

namespace Lucile.ServiceModel
{
    internal class ConectedServiceModel<TService> : IConnected<TService>
    {
        private RemoteServiceOptions _remoteServiceOptions;

        public ConectedServiceModel(RemoteServiceOptions remoteServiceOptions)
        {
            _remoteServiceOptions = remoteServiceOptions;
        }

        public TService GetService()
        {
            var binding = _remoteServiceOptions.GetBinding<TService>();
            var endpointAddress = _remoteServiceOptions.GetEndpointAddress<TService>();
            var cf = new ChannelFactory<TService>(binding, endpointAddress);
            _remoteServiceOptions?.OnChannelFactoryAction?.Invoke(cf);
            return cf.CreateChannel();
        }
    }
}