using System;
using System.Linq.Expressions;
using Lucile.Dynamic.Convention;
using Lucile.Service;

namespace Lucile.Dynamic.DependencyInjection.Service
{
    public class ServiceScopeProxy<TService> : IConnectedServiceProxy<TService>
        where TService : class
    {
#pragma warning disable RECS0108 // Warns about static fields in generic types - intended behavior
        private static readonly Func<IServiceProvider, TService> _createDelegate;
        private static readonly Type _proxyType;
#pragma warning restore RECS0108 // Warns about static fields in generic types

        static ServiceScopeProxy()
        {
            var dtb = new DynamicTypeBuilder<ScopeProxy<TService>>(assemblyBuilderFactory: ScopeProxyHelper.AssemblyBuilderFactory);
            dtb.AddConvention(new ProxyConvention<TService>());
            _proxyType = dtb.GeneratedType;

            var param = Expression.Parameter(typeof(IServiceProvider), "p");
            var body = Expression.New(_proxyType.GetConstructor(new[] { typeof(IServiceProvider) }), param);
            _createDelegate = Expression.Lambda<Func<IServiceProvider, TService>>(body, param).Compile();
        }

        public TService GetProxy(IServiceProvider serviceProvider)
        {
            return _createDelegate(serviceProvider);
        }
    }
}