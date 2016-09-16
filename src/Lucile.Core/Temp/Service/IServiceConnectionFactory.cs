using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Codeworx.Service
{

    public interface IServiceConnectionFactory<out TChannel> where TChannel : class
    {
        TChannel GetChannel(CancellationToken cancellationToken = default(CancellationToken));
    }
}
