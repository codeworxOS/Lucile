using System;
using System.Threading.Tasks;
using Lucile.Dynamic.DependencyInjection.Service;

namespace Lucile.Dynamic.Test
{
    internal class DelegateSyncProxyMiddlware : ISyncProxyMiddleware
    {
        private readonly Action<SyncMiddlewareContext> _called;

        public DelegateSyncProxyMiddlware(Action<SyncMiddlewareContext> called)
        {
            _called = called;
        }

        public void Invoke(SyncProxyDelegate next, SyncMiddlewareContext context)
        {
            _called(context);
            next(context);
        }
    }
}