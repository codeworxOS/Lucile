using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Emit;
using Lucile.Dynamic.Convention;

namespace Lucile.Dynamic
{
    public class TransactionProxyCreator
    {
        private static TransactionProxyCreator _current;
        private static object _currentLocker = new object();
        private ConcurrentDictionary<CacheKey, Func<object>> _factoryCache;

        private TransactionProxyCreator()
        {
            this._factoryCache = new ConcurrentDictionary<CacheKey, Func<object>>();
        }

        public static TransactionProxyCreator Current
        {
            get
            {
                if (_current == null)
                {
                    lock (_currentLocker)
                    {
                        if (_current == null)
                        {
                            _current = new TransactionProxyCreator();
                        }
                    }
                }

                return _current;
            }
        }

        public T CrateProxy<T>(params T[] targets)
            where T : class, new()
        {
            var key = new CacheKey<T>();
            var factory = this._factoryCache.GetOrAdd(key, CreateFactory);
            var result = (T)factory();
            ((ITransactionProxy<T>)result).SetTargets(targets);
            return result;
        }

        private Func<object> CreateFactory(CacheKey key)
        {
            return key.GetFactory();
        }

        private abstract class CacheKey
        {
            public abstract Func<object> GetFactory();
        }

        private class CacheKey<T> : CacheKey
            where T : class, new()
        {
            private AssemblyBuilder _builder;

            private Type _proxyType;

            public override bool Equals(object obj)
            {
                return obj is CacheKey<T>;
            }

            public override Func<object> GetFactory()
            {
                if (this._proxyType == null)
                {
                    var dtb = new DynamicTypeBuilder<T>();
                    dtb.AddConvention(new TransactionProxyConvention(true));
                    if (!dtb.Conventions.OfType<NotifyPropertyChangedConvention>().Any())
                    {
                        dtb.AddConvention(new NotifyPropertyChangedConvention());
                    }

                    _proxyType = dtb.GeneratedType;
                    _builder = dtb.AssemblyBuilder;
                }

                return Expression.Lambda<Func<T>>(Expression.New(_proxyType)).Compile();
            }

            public override int GetHashCode()
            {
                return 0;
            }
        }
    }
}