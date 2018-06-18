using System;
using System.Threading.Tasks;

namespace Lucile.Dynamic.Interceptor
{
    public class DelegateAsyncMethodInterceptor : IAsyncMethodInterceptor
    {
        private Func<AsyncInterceptionContext, Task> _interceptor;

        public DelegateAsyncMethodInterceptor(Func<AsyncInterceptionContext, Task> interceptor, InterceptionMode mode)
        {
            this._interceptor = interceptor;
            this.InterceptionMode = mode;
        }

        public InterceptionMode InterceptionMode
        {
            get;
            private set;
        }

        public Task ExecuteAsync(AsyncInterceptionContext context)
        {
            return this._interceptor(context);
        }
    }
}