using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Lucile.Dynamic.DependencyInjection.Service;
using Lucile.Dynamic.Interceptor;
using Microsoft.Extensions.DependencyInjection;

namespace Lucile.Dynamic.DependencyInjection
{
    public abstract class ScopeProxy<TService>
        where TService : class
    {
        public ScopeProxy(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public IServiceProvider ServiceProvider { get; }

        [MethodInterceptor(InterceptionMode.InsteadOfBody)]
        public void Intercept(InterceptionContext context)
        {
            var stackItems = ServiceProvider.GetServices<ISyncProxyMiddleware>();
            var stack = new SyncProxyDelegateStack(stackItems);

            using (var scope = ServiceProvider.CreateScope(true))
            {
                var ctx = new SyncMiddlewareContext(ServiceProvider, scope, context);
                stack.Invoke(ctx);
            }
        }

        [MethodInterceptor(InterceptionMode.InsteadOfBody)]
        public async Task InterceptAsync(AsyncInterceptionContext context)
        {
            var stackItems = ServiceProvider.GetServices<IAsyncProxyMiddleware>();

            var stack = new AsyncProxyDelegateStack(stackItems);

            using (var scope = ServiceProvider.CreateScope(true))
            {
                var ctx = new AsyncMiddlewareContext(ServiceProvider, scope, context);
                await stack.Invoke(ctx);
            }
        }

        private class AsyncProxyDelegateStack
        {
            private readonly IReadOnlyList<IAsyncProxyMiddleware> _steps;

            public AsyncProxyDelegateStack(IEnumerable<IAsyncProxyMiddleware> steps)
            {
                this._steps = steps.ToImmutableArray();
            }

            public async Task Invoke(AsyncMiddlewareContext context)
            {
                await Next(context);
            }

            public async Task Next(AsyncMiddlewareContext context, int index = 0)
            {
                if (this._steps.Count > index)
                {
                    await this._steps[index].Invoke(p => Next(p, index + 1), context);
                    return;
                }

                var target = context.Scope.ServiceProvider.GetConnectedService<TService>();
                var result = await context.InterceptionContext.ExecuteBodyOnAsync<TService>(target);
                context.InterceptionContext.SetResult(result);
            }
        }

        private class SyncProxyDelegateStack
        {
            private readonly IReadOnlyList<ISyncProxyMiddleware> _steps;

            public SyncProxyDelegateStack(IEnumerable<ISyncProxyMiddleware> steps)
            {
                this._steps = steps.ToImmutableArray();
            }

            public void Invoke(SyncMiddlewareContext context)
            {
                Next(context);
            }

            public void Next(SyncMiddlewareContext context, int index = 0)
            {
                if (this._steps.Count > index)
                {
                    this._steps[index].Invoke(p => Next(p, index + 1), context);
                    return;
                }

                var target = context.Scope.ServiceProvider.GetConnectedService<TService>();
                var result = context.InterceptionContext.ExecuteBodyOn<TService>(target);
                context.InterceptionContext.SetResult(result);
            }
        }
    }
}