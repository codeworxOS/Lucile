using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Codeworx.Dynamic;

namespace Codeworx.Service
{
    public class ChannelProxyCache
    {
        private class ChannelProxyAssemblyBuilderFactory : AssemblyBuilderFactory
        {
            public override bool CanPersist
            {
                get
                {
                    return false;
                }
            }

            private Guid assemblyGuid;

            AssemblyBuilder builder;

            ConcurrentDictionary<string, int> typeNames;

            public ChannelProxyAssemblyBuilderFactory()
            {
                this.typeNames = new ConcurrentDictionary<string, int>();

                this.assemblyGuid = Guid.NewGuid();
                var an = new AssemblyName(string.Format("Codeworx.ChannelProxies_{0}", this.assemblyGuid));
#if (!SILVERLIGHT)
#if(DEBUGDYNAMIC)
                builder = AppDomain.CurrentDomain.DefineDynamicAssembly(an, AssemblyBuilderAccess.RunAndSave);
#else
                builder = AppDomain.CurrentDomain.DefineDynamicAssembly(an, AssemblyBuilderAccess.RunAndCollect);
#endif
#else
                builder = AppDomain.CurrentDomain.DefineDynamicAssembly(an, AssemblyBuilderAccess.Run);
#endif
            }

            public override System.Reflection.Emit.AssemblyBuilder GetAssemblyBuilder()
            {
                return builder;
            }

            public override string GetUniqueTypeName(Type baseType)
            {
                var typeName = string.Format("Codeworx.ChannelProxies_{0}.{1}_dynamic", this.assemblyGuid, baseType.Name);

                var count = typeNames.AddOrUpdate(typeName, 0, (p, q) => q + 1);
                if (count > 0) {
                    typeName = string.Format("{0}_{1}", typeName, count);
                }

                return typeName;
            }


        }


        private Collection<object> items;

        private object itemsLocker;

        AssemblyBuilderFactory assemblyBuilderFactory;

        private ChannelProxyCache()
        {
            this.assemblyBuilderFactory = new ChannelProxyAssemblyBuilderFactory();
            this.itemsLocker = new object();
            this.items = new Collection<object>();
        }

        private static object syncRoot = new object();

        private static ChannelProxyCache current;

        public static ChannelProxyCache Current
        {
            get
            {
                if (current == null) {
                    lock (syncRoot) {
                        if (current == null) {
                            current = new ChannelProxyCache();
                        }
                    }
                }
                return current;
            }
        }

        public ChannelProxyCacheItem<TChannel, TProxy> GetOrAdd<TChannel, TProxy>()
            where TChannel : class
            where TProxy : ChannelProxy<TChannel>
        {
            ChannelProxyCacheItem<TChannel, TProxy> item;
            lock (itemsLocker) {
                item = items.OfType<ChannelProxyCacheItem<TChannel, TProxy>>().FirstOrDefault();
                if (item == null) {
                    item = new ChannelProxyCacheItem<TChannel, TProxy>(assemblyBuilderFactory);
                    items.Add(item);
                }
            }

            return item;
        }
    }
}
