using System;
using System.Collections.Generic;
using System.Linq;

namespace Lucile.Mapper
{
    public static class MappingExtensions
    {
        public static TTarget Map<TSource, TTarget>(this TSource source)
        {
            throw new NotSupportedException("The Map extension method is not intended to be used directly. Only use it within other mappings oder in queries");
        }

        public static IQueryable<TTarget> Map<TSource, TTarget>(this IQueryable<TSource> query)
        {
            throw new NotSupportedException("The Map extension method is not intended to be used directly. Only use it within other mappings oder in queries");
        }

        public static IEnumerable<TTarget> Map<TSource, TTarget>(this IEnumerable<TSource> source)
        {
            throw new NotSupportedException("The Map extension method is not intended to be used directly. Only use it within other mappings oder in queries");
        }
    }
}
