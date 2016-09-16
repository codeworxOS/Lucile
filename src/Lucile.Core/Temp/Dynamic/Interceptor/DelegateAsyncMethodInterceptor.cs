using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeworx.Dynamic.Interceptor
{
    public class DelegateAsyncMethodInterceptor : IAsyncMethodInterceptor
    {
        private Func<AsyncInterceptionContext, Task> interceptor;

        public DelegateAsyncMethodInterceptor(Func<AsyncInterceptionContext,Task> interceptor, InterceptionMode mode)
        {
            this.interceptor = interceptor;
            this.InterceptionMode = mode;
        }

        #region IAsyncMethodInterceptor Members

        public Task ExecuteAsync(AsyncInterceptionContext context)
        {
            return this.interceptor(context);
        }

        #endregion

        #region IMethodInterceptorBase Members

        public InterceptionMode InterceptionMode
        {
            get;
            private set;
        }

        #endregion
    }
}
