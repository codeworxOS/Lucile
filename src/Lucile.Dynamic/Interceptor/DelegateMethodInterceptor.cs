using System;

namespace Lucile.Dynamic.Interceptor
{
    public class DelegateMethodInterceptor : IMethodInterceptor
    {
        private Action<InterceptionContext> _interceptor;

        public DelegateMethodInterceptor(Action<InterceptionContext> interceptor, InterceptionMode mode)
        {
            this._interceptor = interceptor;
            this.InterceptionMode = mode;
        }

        public InterceptionMode InterceptionMode
        {
            get;
            private set;
        }

        public void Execute(InterceptionContext context)
        {
            _interceptor(context);
        }
    }
}