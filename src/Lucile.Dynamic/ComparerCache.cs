using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Lucile.Dynamic
{
    internal class ComparerCache
    {
        private static ConcurrentDictionary<object, object> _cache;

        static ComparerCache()
        {
            _cache = new ConcurrentDictionary<object, object>();
            _cache.GetOrAdd(TypeKey<byte[]>.Key, new ByteArrayComparer());
        }

        public static IEqualityComparer<TType> Get<TType>()
        {
            if (_cache.TryGetValue(TypeKey<TType>.Key, out var value))
            {
                return value as IEqualityComparer<TType>;
            }

            return null;
        }

        public static void Register<TType>(IEqualityComparer<TType> comparer)
        {
            _cache.AddOrUpdate(TypeKey<TType>.Key, comparer, (p, q) => comparer);
        }
    }
}