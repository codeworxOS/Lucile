using System;
using Lucile.Dynamic.Interceptor;
using Microsoft.Extensions.DependencyInjection;

namespace Lucile.Dynamic.DependencyInjection.Service
{
    public class SyncMiddlewareContext
    {
        public SyncMiddlewareContext(IServiceProvider rootServiceProvider, IServiceScope scope, InterceptionContext interceptionContext)
        {
            RootServiceProvider = rootServiceProvider;
            Scope = scope;
            InterceptionContext = interceptionContext;
        }

        public InterceptionContext InterceptionContext { get; }

        public IServiceProvider RootServiceProvider { get; }

        public IServiceScope Scope { get; }

        public IServiceProvider ServiceProvider => Scope?.ServiceProvider ?? RootServiceProvider;
    }
}