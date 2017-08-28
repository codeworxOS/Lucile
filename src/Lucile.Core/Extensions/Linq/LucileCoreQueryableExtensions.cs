using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Lucile.Linq;
using Lucile.Linq.Configuration;
using Lucile.Reflection;

namespace System.Linq
{
    public static class LucileCoreQueryableExtensions
    {
        public static IQueryable<TElement> Apply<TElement>(this IQueryable<TElement> query, QueryConfiguration config)
        {
            FilterItem item = config.FilterItems.Count > 1 ? new FilterItemGroup(config.FilterItems) : config.FilterItems.FirstOrDefault();

            if (item != null)
            {
                query = query.ApplyFilterItem(item);
            }

            if (config.Sort.Any())
            {
                query = query.ApplySort(config.Sort);
            }

            return query;
        }

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

        public static IQueryable<TElement> ApplySort<TElement>(this IQueryable<TElement> query, IEnumerable<SortItem> sort)
        {
            return (IQueryable<TElement>)ApplySort((IQueryable)query, sort);
        }

        public static IQueryable ApplySort(this IQueryable query, IEnumerable<SortItem> sort)
        {
            var expr = query.Expression;
            var elementType = query.ElementType;

            MethodInfo method = null;

            var param = Expression.Parameter(elementType);

            foreach (var item in sort)
            {
                if (method == null && item.SortDirection == SortDirection.Ascending)
                {
                    method = QueryableInfo.OrderBy;
                }
                else if (method == null && item.SortDirection == SortDirection.Descending)
                {
                    method = QueryableInfo.OrderByDescending;
                }
                else if (method != null && item.SortDirection == SortDirection.Ascending)
                {
                    method = QueryableInfo.ThenBy;
                }
                else if (method != null && item.SortDirection == SortDirection.Descending)
                {
                    method = QueryableInfo.ThenByDescending;
                }
                else
                {
                    throw new InvalidOperationException($"No mehtod for SortDirection {item.SortDirection} found");
                }

                Expression body = null;

                foreach (var prop in item.PropertyPath.Split('.'))
                {
                    body = Expression.Property(body ?? param, prop);
                }

                method = method.MakeGenericMethod(elementType, body.Type);

                expr = Expression.Call(method, expr, Expression.Quote(Expression.Lambda(body, param)));
            }

            return query.Provider.CreateQuery(expr);
        }

        public static IQueryable<TElement> ApplySort<TElement>(this IQueryable<TElement> query, params SortItem[] sort)
        {
            return ApplySort<TElement>(query, (IEnumerable<SortItem>)sort);
        }
    }
}