using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeworx.Dynamic.Interceptor
{
    public class DelegateMethodInterceptor : IMethodInterceptor
    {
        private Action<InterceptionContext> interceptor;

        public DelegateMethodInterceptor(Action<InterceptionContext> interceptor, InterceptionMode mode)
        {
            this.interceptor = interceptor;
            this.InterceptionMode = mode;
        }

        public void Execute(InterceptionContext context)
        {
            interceptor(context);
        }

        public InterceptionMode InterceptionMode
        {
            get;
            private set;
        }
    }
}
