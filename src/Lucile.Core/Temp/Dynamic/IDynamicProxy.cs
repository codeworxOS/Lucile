using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeworx.Dynamic
{
    public interface IDynamicProxy
    {
        void SetProxyTarget<T>(T target) where T : class;
        T GetProxyTarget<T>() where T : class;
    }
}
