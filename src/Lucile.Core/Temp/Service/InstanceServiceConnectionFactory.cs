using Codeworx.ComponentModel;
using System.Threading;
using Codeworx.Dynamic;

namespace Codeworx.Service
{
    public class InstanceServiceConnectionFactory<T> : IServiceConnectionFactory<T> where T : class
    {
        public InstanceServiceConnectionFactory()
        {

        }

        public T GetChannel(CancellationToken cancellationToken = default(CancellationToken))
        {
            return CompositionContext.Current.GetExport<T>();
        }
    }
}
