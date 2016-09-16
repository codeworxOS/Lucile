using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.ServiceModel;
using System.Text;
using Codeworx.Dynamic;
using Codeworx.Dynamic.Convention;
using Codeworx.Dynamic.Interceptor;

namespace Codeworx.Service
{
    public class ChannelProxyCacheItem<TChannel, TProxy> where TChannel : class where TProxy : ChannelProxy<TChannel>
    {
        private Func<TProxy> createProxyDelegate;

        private Type proxyType;

        public bool HasCallback { get; private set; }

        public Type CallbackType { get; private set; }

        public ChannelProxyCacheItem(AssemblyBuilderFactory assemblyFactory)
        {
            var serviceContract = typeof(TChannel).GetCustomAttributes(typeof(ServiceContractAttribute), true).OfType<ServiceContractAttribute>().FirstOrDefault();
            if (serviceContract != null) {
                this.HasCallback = serviceContract.CallbackContract != null;
                this.CallbackType = serviceContract.CallbackContract;
            }

            var members = new List<DynamicMember>();

            if (HasCallback) {
                foreach (var item in CallbackType.GetInterfaces().Union(new[] { CallbackType })) {

                    if (item.GetProperties().Any() ||
                        item.GetEvents().Any() ||
                        item.GetMethods().Where(p => p.ReturnType != typeof(void)).Any()) {
                        throw new InvalidOperationException("Only methods and only void retrun types are allowed for callback contracts.");
                    }

                    members.AddRange(item.GetMethods().Select(p => new DynamicVoid(p.Name, p.GetParameters().Select(x => x.ParameterType).ToArray())));
                }
            }

            var dtb = new DynamicTypeBuilder<TProxy>(members, assemblyFactory);
            dtb.AddConvention(new ProxyConvention<TChannel>());
            if (HasCallback) {
                dtb.AddInterceptor(new ImplementInterfaceInterceptor(this.CallbackType));
            }

            this.proxyType = dtb.GeneratedType;

            createProxyDelegate = Expression.Lambda<Func<TProxy>>(Expression.New(this.proxyType)).Compile();
        }

        public TProxy GetProxy() {
            return createProxyDelegate();
        }
    }
}
