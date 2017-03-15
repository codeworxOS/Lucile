using System;

namespace Lucile.Service
{
    internal class LocalServiceImplementation<TService, TImplmentation> : ILocal<TService>
        where TService : class
        where TImplmentation : class, TService
    {
        private readonly IServiceProvider _serviceProvider;

        public LocalServiceImplementation(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IConnected<TService> GetConnectedService()
        {
            return new ConnectedInstance<TService, TImplmentation>(_serviceProvider);
        }
    }
}