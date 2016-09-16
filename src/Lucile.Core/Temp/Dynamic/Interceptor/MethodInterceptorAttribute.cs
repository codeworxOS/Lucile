using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeworx.Dynamic.Interceptor
{
    [AttributeUsage(AttributeTargets.Method)]
    public class MethodInterceptorAttribute : Attribute
    {
        public MethodInterceptorAttribute(InterceptionMode mode)
        {
            this.InterceptionMode = mode;
        }

        public InterceptionMode InterceptionMode { get; private set; }
    }
}
