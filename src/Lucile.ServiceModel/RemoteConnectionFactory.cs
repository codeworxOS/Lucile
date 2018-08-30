using System;
using Lucile.Service;

namespace Lucile.ServiceModel
{
    internal class RemoteConnectionFactory : IConnectionFactory
    {
        private RemoteServiceOptions _remoteServiceOptions;

        public RemoteConnectionFactory(RemoteServiceOptions remoteServiceOptions)
        {
            this._remoteServiceOptions = remoteServiceOptions;
        }

        public IConnected<TService> GetConnectedService<TService>(IServiceProvider provider)
        {
            return new ConnectedServiceModel<TService>(this._remoteServiceOptions);
        }
    }
}