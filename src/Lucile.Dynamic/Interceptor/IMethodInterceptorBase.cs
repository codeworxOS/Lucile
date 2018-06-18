using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lucile.Dynamic.Interceptor
{
    public interface IMethodInterceptorBase
    {
        InterceptionMode InterceptionMode { get; }
    }
}
