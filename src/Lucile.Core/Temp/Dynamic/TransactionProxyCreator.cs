using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Codeworx.Dynamic.Convention;

namespace Codeworx.Dynamic
{
    public class TransactionProxyCreator
    {
        private abstract class CacheKey
        {
            public abstract Func<object> GetFactory();
        }

        private class CacheKey<T> : CacheKey where T : class, new()
        {
            public override int GetHashCode()
            {
                return 0;
            }
            public override bool Equals(object obj)
            {
                return obj is CacheKey<T>;
            }

            private AssemblyBuilder builder;

            private Type proxyType;

            public override Func<object> GetFactory()
            {
                if (this.proxyType == null) {
                    var dtb = new DynamicTypeBuilder<T>();
                    dtb.AddConvention(new TransactionProxyConvention(true));
                    if (!dtb.Conventions.OfType<NotifyPropertyChangedConvention>().Any()) {
                        dtb.AddConvention(new NotifyPropertyChangedConvention());
                    }

                    proxyType = dtb.GeneratedType;
                    builder = dtb.AssemblyBuilder;
                }

                return Expression.Lambda<Func<T>>(Expression.New(proxyType)).Compile();
            }
        }

        private ConcurrentDictionary<CacheKey, Func<object>> factoryCache;

        private TransactionProxyCreator()
        {
            this.factoryCache = new ConcurrentDictionary<CacheKey, Func<object>>();
        }

        public T CrateProxy<T>(params T[] targets) where T : class, new()
        {
            var key = new CacheKey<T>();
            var factory = this.factoryCache.GetOrAdd(key, CreateFactory);
            var result = (T)factory();
            ((ITransactionProxy<T>)result).SetTargets(targets);
            return result;
        }

        private Func<object> CreateFactory(CacheKey key)
        {
            return key.GetFactory();
        }

        private static TransactionProxyCreator current;
        private static object currentLocker = new object();
        public static TransactionProxyCreator Current
        {
            get
            {
                if (current == null) {
                    lock (currentLocker) {
                        if (current == null) {
                            current = new TransactionProxyCreator();
                        }
                    }
                }
                return current;
            }
        }
    }
}
