using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lucile.Dynamic.Interceptor
{
    public interface IAsyncMethodInterceptor : IMethodInterceptorBase
    {
        Task ExecuteAsync(AsyncInterceptionContext context);
    }
}
