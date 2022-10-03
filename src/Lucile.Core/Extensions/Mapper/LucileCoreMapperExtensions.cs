using System.Linq;

namespace Lucile.Mapper
{
    public static class LucileCoreMapperExtensions
    {
        public static IQueryable<TTarget> Query<TSource, TTarget>(this IMapper<TSource, TTarget> mapper, IQueryable<TSource> queryable)
        {
            return queryable.Select(mapper.Configuration.Expression);
        }
    }
}
