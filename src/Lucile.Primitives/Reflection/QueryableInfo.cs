using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Lucile.Reflection
{
    public class QueryableInfo
    {
        static QueryableInfo()
        {
            Expression<Func<IQueryable<object>, IQueryable<object>>> querySelect = p => p.Select(x => x);
            Select = ((MethodCallExpression)querySelect.Body).Method.GetGenericMethodDefinition();
            Expression<Func<IQueryable<object>, IQueryable<object>>> querySelectMany = p => p.SelectMany(x => new[] { x }, (parent, child) => child);
            SelectMany = ((MethodCallExpression)querySelectMany.Body).Method.GetGenericMethodDefinition();
            Expression<Func<IQueryable<object>, IQueryable<object>>> queryGroupJoin = p => p.GroupJoin(Enumerable.Empty<object>(), x => x, x => x, (parent, child) => parent);
            GroupJoin = ((MethodCallExpression)queryGroupJoin.Body).Method.GetGenericMethodDefinition();
            Expression<Func<IQueryable<object>, IQueryable<object>>> queryWhere = p => p.Where(x => true);
            Where = ((MethodCallExpression)queryWhere.Body).Method.GetGenericMethodDefinition();
            Expression<Func<IQueryable<object>, IQueryable<object>>> queryOrderBy = p => p.OrderBy(x => Guid.NewGuid());
            OrderBy = ((MethodCallExpression)queryOrderBy.Body).Method.GetGenericMethodDefinition();
            Expression<Func<IQueryable<object>, IQueryable<object>>> queryOrderByDesc = p => p.OrderByDescending(x => Guid.NewGuid());
            OrderByDescending = ((MethodCallExpression)queryOrderByDesc.Body).Method.GetGenericMethodDefinition();
            Expression<Func<IOrderedQueryable<object>, IOrderedQueryable<object>>> queryThenBy = p => p.ThenBy(x => Guid.NewGuid());
            ThenBy = ((MethodCallExpression)queryThenBy.Body).Method.GetGenericMethodDefinition();
            Expression<Func<IOrderedQueryable<object>, IOrderedQueryable<object>>> queryThenByDesc = p => p.ThenByDescending(x => Guid.NewGuid());
            ThenByDescending = ((MethodCallExpression)queryThenByDesc.Body).Method.GetGenericMethodDefinition();
        }

        public static MethodInfo GroupJoin { get; }

        public static MethodInfo OrderBy { get; }

        public static MethodInfo OrderByDescending { get; }

        public static MethodInfo Select { get; }

        public static MethodInfo SelectMany { get; }

        public static MethodInfo ThenBy { get; }

        public static MethodInfo ThenByDescending { get; }

        public static MethodInfo Where { get; }
    }
}