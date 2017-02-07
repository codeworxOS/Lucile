using System.Linq.Expressions;
using Lucile.Linq.Configuration;
using Lucile.Reflection;

namespace System.Linq
{
    public static class LucileCoreQueryableExtensions
    {
        public static IQueryable<TElement> ApplyFilterItem<TElement>(this IQueryable<TElement> query, FilterItem filter)
        {
            return (IQueryable<TElement>)ApplyFilterItem((IQueryable)query, filter);
        }

        public static IQueryable ApplyFilterItem(this IQueryable query, FilterItem filter)
        {
            var exp = query.Expression;
            var param = Expression.Parameter(query.ElementType);
            var body = filter.GetExpression(param);
            if (body != null)
            {
                var whereLambdaExpression = Expression.Lambda(body, param);
                var whereExpression = Expression.Call(QueryableInfo.Where.MakeGenericMethod(param.Type), exp, whereLambdaExpression);

                return query.Provider.CreateQuery(whereExpression);
            }

            return query;
        }
    }
}