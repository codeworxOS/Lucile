using System;
using System.Linq;
using System.Reflection;
using Lucile.Dynamic.Interceptor;
using Lucile.Dynamic.Methods;

namespace Lucile.Dynamic.Convention
{
    public class ProxyConvention : DynamicTypeConvention, IProxyConvention
    {
        public ProxyConvention(Type proxyType)
        {
            this.ProxyType = proxyType;
            this.ProxyTarget = new DynamicProperty(Guid.NewGuid().ToString(), proxyType);
        }

        public DynamicProperty ProxyTarget { get; private set; }

        public Type ProxyType { get; private set; }

        public override void Apply(DynamicTypeBuilder typeBuilder)
        {
            if (!this.ProxyType.GetTypeInfo().IsInterface)
            {
                throw new InvalidOperationException("The generic argument T must be an interface!");
            }

            var prop = this.ProxyTarget;
            typeBuilder.AddMember(prop);

            var interfaces = this.ProxyType.GetInterfaces().Union(new[] { this.ProxyType });

            foreach (var item in interfaces.SelectMany(p => p.GetMethods()).Distinct())
            {
                typeBuilder.AddMember(new ProxyMethod(item, prop));
            }

            foreach (var item in interfaces.SelectMany(p => p.GetProperties()).Distinct())
            {
                typeBuilder.AddMember(new DynamicProxyProperty(item, prop));
            }

            foreach (var item in interfaces.SelectMany(p => p.GetEvents()).Distinct())
            {
                typeBuilder.AddMember(new DynamicProxyEvent(item, prop));
            }

            if (!typeBuilder.DynamicMembers.OfType<SetProxyTargetMethod>().Any())
            {
                typeBuilder.AddMember(new SetProxyTargetMethod());
            }

            if (!typeBuilder.DynamicMembers.OfType<GetProxyTargetMethod>().Any())
            {
                typeBuilder.AddMember(new GetProxyTargetMethod());
            }

            foreach (var item in interfaces)
            {
                typeBuilder.AddInterceptor(new ImplementInterfaceInterceptor(item));
            }

            typeBuilder.AddInterceptor(new ImplementInterfaceInterceptor<IDynamicProxy>());
        }
    }
}