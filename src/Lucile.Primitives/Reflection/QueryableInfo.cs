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
        }

        public static MethodInfo GroupJoin { get; }

        public static MethodInfo Select { get; }

        public static MethodInfo SelectMany { get; }

        public static MethodInfo Where { get; }
    }
}