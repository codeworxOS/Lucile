using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Codeworx.Dynamic.Interceptor;
using Codeworx.Dynamic.Methods;

namespace Codeworx.Dynamic.Convention
{
    public class ProxyConvention<T> : ProxyConvention where T : class {
        public ProxyConvention() : base(typeof(T))
        {

        }
    }

    public class ProxyConvention : DynamicTypeConvention, IProxyConvention
    {
        public DynamicProperty ProxyTarget { get; private set; }

        public Type ProxyType { get; private set; }

        public ProxyConvention(Type proxyType)
        {
            this.ProxyType = proxyType;
            this.ProxyTarget = new DynamicProperty(Guid.NewGuid().ToString(), proxyType);
        }

        public override void Apply(DynamicTypeBuilder typeBuilder)
        {
            if (!this.ProxyType.IsInterface)
                throw new InvalidOperationException("The generic argument T must be an interface!");

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
                typeBuilder.AddMember(new SetProxyTargetMethod());

            if (!typeBuilder.DynamicMembers.OfType<GetProxyTargetMethod>().Any())
                typeBuilder.AddMember(new GetProxyTargetMethod());

            foreach (var item in interfaces) { 
                typeBuilder.AddInterceptor(new ImplementInterfaceInterceptor(item));
            }
            typeBuilder.AddInterceptor(new ImplementInterfaceInterceptor<IDynamicProxy>());
        }
    }
}
