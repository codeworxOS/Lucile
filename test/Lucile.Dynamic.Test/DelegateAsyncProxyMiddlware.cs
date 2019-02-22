using System;
using System.Threading.Tasks;
using Lucile.Dynamic.DependencyInjection.Service;

namespace Lucile.Dynamic.Test
{
    internal class DelegateAsyncProxyMiddlware : IAsyncProxyMiddleware
    {
        private readonly Action<AsyncMiddlewareContext> _called;

        public DelegateAsyncProxyMiddlware(Action<AsyncMiddlewareContext> called)
        {
            _called = called;
        }

        public async Task Invoke(AsyncProxyDelegate next, AsyncMiddlewareContext context)
        {
            _called(context);
            await next(context);
        }
    }
}