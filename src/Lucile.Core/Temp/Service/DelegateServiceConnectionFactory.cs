using System;
using System.Threading;

namespace Codeworx.Service
{
    public class DelegateServiceConnectionFactory<TService> : IServiceConnectionFactory<TService>
        where TService : class
    {
        private Func<CancellationToken,TService> factory;

        public DelegateServiceConnectionFactory(Func<CancellationToken, TService> factory)
        {
            this.factory = factory;
        }

        public TService GetChannel(System.Threading.CancellationToken cancellationToken = default(CancellationToken))
        {
            return this.factory(cancellationToken);
        }
    }
}
