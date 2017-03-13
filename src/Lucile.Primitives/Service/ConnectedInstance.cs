using System;
using Microsoft.Extensions.DependencyInjection;

namespace Lucile.Service
{
    internal class ConnectedInstance<TService, TImplementation> : IConnected<TService>
        where TService : class
        where TImplementation : TService
    {
        private readonly IServiceProvider _provider;

        public ConnectedInstance(IServiceProvider provider)
        {
            _provider = provider;
        }

        public TService GetService()
        {
            return _provider.GetService<TImplementation>();
        }
    }
}