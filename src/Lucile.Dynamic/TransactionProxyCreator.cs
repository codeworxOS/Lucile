using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

        public T CreateProxy<T>(params T[] targets)
            where T : class
        {
            return CreateProxy<T>((IEnumerable<T>)targets);
        }

        public T CreateProxy<T>(IEnumerable<T> targets)
            where T : class
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

        private class CacheKey<T> : CacheKey, IEquatable<CacheKey<T>>
            where T : class
        {
#pragma warning disable RECS0108 // Warns about static fields in generic types - this is intentional
            private static AssemblyBuilderFactory _currentFactory;
            private static object _currentFactoryLocker = new object();
            private static object _key = new object();
#pragma warning restore RECS0108 // Warns about static fields in generic types - this is intentional

            public AssemblyBuilderFactory CurrentFactory
            {
                get
                {
                    if (_currentFactory == null)
                    {
                        lock (_currentFactoryLocker)
                        {
                            if (_currentFactory == null)
                            {
                                _currentFactory = new StaticAssemblyBuilderFactory("Lucile.Dynamic.TransactionProxy");
                            }
                        }
                    }

                    return _currentFactory;
                }
            }

            protected object Key => _key;

            public override bool Equals(object obj)
            {
                var other = obj as CacheKey<T>;
                if (other != null)
                {
                    return Equals(other);
                }

                return false;
            }

            public bool Equals(CacheKey<T> other)
            {
                return Key == other.Key;
            }

            public override Func<object> GetFactory()
            {
                DynamicTypeBuilder dtb = null;

                if (typeof(T).IsInterface)
                {
                    dtb = new DynamicTypeBuilder<DynamicObjectBase>(assemblyBuilderFactory: CurrentFactory);
                }
                else
                {
                    dtb = new DynamicTypeBuilder<T>(assemblyBuilderFactory: CurrentFactory);
                    if (typeof(T).GetConstructor(new Type[] { }) == null)
                    {
                        throw new InvalidOperationException("The base class must have a parameterless constructor.");
                    }
                }

                dtb.AddConvention(new TransactionProxyConvention(typeof(T), true));
                if (!dtb.Conventions.OfType<NotifyPropertyChangedConvention>().Any())
                {
                    dtb.AddConvention(new NotifyPropertyChangedConvention());
                }

                if (typeof(T).IsInterface)
                {
                    dtb.AddConvention(new ImplementInterfaceConvention(typeof(T)));
                    foreach (var childInterfaces in typeof(T).GetInterfaces())
                    {
                        dtb.AddConvention(new ImplementInterfaceConvention(childInterfaces));
                    }
                }

                return Expression.Lambda<Func<T>>(Expression.New(dtb.GeneratedType)).Compile();
            }

            public override int GetHashCode()
            {
                return Key.GetHashCode();
            }
        }
    }
}