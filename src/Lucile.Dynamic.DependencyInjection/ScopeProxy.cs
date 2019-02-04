using System;
using System.Threading.Tasks;
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
            using (var scope = ServiceProvider.CreateScope(true))
            {
                var target = scope.ServiceProvider.GetConnectedService<TService>();
                var result = context.ExecuteBodyOn<TService>(target);
                context.SetResult(result);
            }
        }

        [MethodInterceptor(InterceptionMode.InsteadOfBody)]
        public async Task InterceptAsync(AsyncInterceptionContext context)
        {
            using (var scope = ServiceProvider.CreateScope(true))
            {
                var target = scope.ServiceProvider.GetConnectedService<TService>();
                var result = await context.ExecuteBodyOnAsync<TService>(target);
                context.SetResult(result);
            }
        }
    }
}