using System;

namespace Lucile.Service
{
    public interface IConnectedServiceProxy<TService>
        where TService : class
    {
        TService GetProxy(IServiceProvider serviceProvider);
    }
}