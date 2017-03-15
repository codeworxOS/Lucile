using System;
using Microsoft.Extensions.DependencyInjection;

namespace Lucile.Service
{
    internal class LocalConnectionFactory : IConnectionFactory
    {
        public IConnected<TService> GetConnectedService<TService>(IServiceProvider provider)
        {
            var local = provider.GetService<ILocal<TService>>();
            return local?.GetConnectedService();
        }
    }
}