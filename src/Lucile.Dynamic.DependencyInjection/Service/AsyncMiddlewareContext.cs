using System;
using Lucile.Dynamic.Interceptor;
using Microsoft.Extensions.DependencyInjection;

namespace Lucile.Dynamic.DependencyInjection.Service
{
    public class AsyncMiddlewareContext
    {
        public AsyncMiddlewareContext(IServiceProvider rootServiceProvider, IServiceScope scope, AsyncInterceptionContext interceptionContext)
        {
            RootServiceProvider = rootServiceProvider;
            Scope = scope;
            InterceptionContext = interceptionContext;
        }

        public AsyncInterceptionContext InterceptionContext { get; }

        public IServiceProvider RootServiceProvider { get; }

        public IServiceScope Scope { get; }

        public IServiceProvider ServiceProvider => Scope?.ServiceProvider ?? RootServiceProvider;
    }
}