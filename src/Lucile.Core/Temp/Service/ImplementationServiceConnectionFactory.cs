using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Codeworx.Service
{
    public class ImplementationServiceConnectionFactory<TService,TImplementation> : IServiceConnectionFactory<TService> where TService : class where TImplementation : TService, new()
    {
        public TService GetChannel(System.Threading.CancellationToken cancellationToken = default(CancellationToken))
        {
            return new TImplementation();
        }
    }
}
