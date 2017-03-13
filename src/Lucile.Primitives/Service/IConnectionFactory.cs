using System;

namespace Lucile.Service
{
    public interface IConnectionFactory
    {
        IConnected<TService> GetConnectedService<TService>(IServiceProvider provider);
    }
}