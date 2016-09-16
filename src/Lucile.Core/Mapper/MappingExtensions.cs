using System.Collections.Generic;
using System.Linq;
using Lucile.Mapper;

namespace System
{
    public static class MappingExtensions
    {
        public static TTarget Map<TSource, TTarget>(this TSource source)
        {
            return MappingContainer.Current.Map<TSource, TTarget>(source);
        }

        public static bool TryMap<TSource, TTarget>(this TSource source, out TTarget target)
        {
            return MappingContainer.Current.TryMap<TSource, TTarget>(source, out target);
        }

        public static IQueryable<TTarget> Map<TSource, TTarget>(this IQueryable<TSource> query)
        {
            return MappingContainer.Current.Map<TSource, TTarget>(query);
        }

        public static IEnumerable<TTarget> Map<TSource, TTarget>(this IEnumerable<TSource> source)
        {
            return MappingContainer.Current.Map<TSource, TTarget>(source);
        }
    }
}
