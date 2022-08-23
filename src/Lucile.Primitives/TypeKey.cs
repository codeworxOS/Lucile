using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace Lucile
{
    public class TypeKey
    {
        private static readonly ConcurrentDictionary<Type, object> _keyCache = new ConcurrentDictionary<Type, object>();

        public static object ForType(Type type)
        {
            return _keyCache.GetOrAdd(type, p => typeof(TypeKey<>).MakeGenericType(p).GetProperty("Key", BindingFlags.Static | BindingFlags.Public).GetValue(null));
        }
    }
}