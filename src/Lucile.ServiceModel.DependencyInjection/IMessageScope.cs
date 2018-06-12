using System;

namespace Lucile.ServiceModel.DependencyInjection
{
    public interface IMessageScope : IDisposable
    {
        void Register(IServiceProvider provider);
    }
}