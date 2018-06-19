using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lucile.Dynamic.Interceptor;

namespace Lucile.Dynamic.Test.Dynamic
{
    public delegate void InterceptorEvent(InterceptionMode mode, InterceptionContextBase context);

    public class DynamicAsyncProxyBase
    {
        public event InterceptorEvent InterceptorCalled;

        [MethodInterceptor(Lucile.Dynamic.Interceptor.InterceptionMode.AfterBody)]
        protected Task AfterInterceptorAsync(AsyncInterceptionContext context)
        {
            return Task.Run(() => OnInterceptorCalled(InterceptionMode.AfterBody, context));
        }

        [MethodInterceptor(Lucile.Dynamic.Interceptor.InterceptionMode.BeforeBody)]
        protected Task BeforeInterceptorAsync(AsyncInterceptionContext context)
        {
            return Task.Run(() => OnInterceptorCalled(InterceptionMode.BeforeBody, context));
        }

        [MethodInterceptor(Lucile.Dynamic.Interceptor.InterceptionMode.InsteadOfBody)]
        protected async Task InsteadInterceptorAsync(AsyncInterceptionContext context)
        {
            if (context.HasResult)
            {
                context.SetResult(await context.ExecuteBodyAsync());
                await Task.Run(() => OnInterceptorCalled(InterceptionMode.InsteadOfBody, context));
            }
        }

        protected virtual void OnInterceptorCalled(InterceptionMode mode, InterceptionContextBase context)
        {
            if (InterceptorCalled != null)
                InterceptorCalled(mode, context);
        }
    }

    public class DynamicProxyBase
    {
        public event InterceptorEvent InterceptorCalled;

        [MethodInterceptor(Lucile.Dynamic.Interceptor.InterceptionMode.AfterBody)]
        protected void AfterInterceptor(InterceptionContext context)
        {
            OnInterceptorCalled(InterceptionMode.AfterBody, context);
        }

        [MethodInterceptor(Lucile.Dynamic.Interceptor.InterceptionMode.BeforeBody)]
        protected void BeforeInterceptor(InterceptionContext context)
        {
            OnInterceptorCalled(InterceptionMode.BeforeBody, context);
        }

        [MethodInterceptor(Lucile.Dynamic.Interceptor.InterceptionMode.InsteadOfBody)]
        protected void InsteadInterceptor(InterceptionContext context)
        {
            context.SetResult(context.ExecuteBody());
            OnInterceptorCalled(InterceptionMode.InsteadOfBody, context);
        }

        protected virtual void OnInterceptorCalled(InterceptionMode mode, InterceptionContextBase context)
        {
            if (InterceptorCalled != null)
                InterceptorCalled(mode, context);
        }
    }
}