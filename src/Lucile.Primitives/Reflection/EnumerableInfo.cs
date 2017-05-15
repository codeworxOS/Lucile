using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Lucile.Reflection
{
    public class EnumerableInfo
    {
        static EnumerableInfo()
        {
            Expression<Func<IEnumerable<object>, IEnumerable<object>>> enumDefaultIfEmptyJoin = p => p.DefaultIfEmpty();
            DefaultIfEmpty = ((MethodCallExpression)enumDefaultIfEmptyJoin.Body).Method.GetGenericMethodDefinition();

            Expression<Func<IEnumerable<object>, bool>> enumAny = p => p.Any();
            Any = ((MethodCallExpression)enumAny.Body).Method.GetGenericMethodDefinition();

            Expression<Func<IEnumerable<object>, bool>> enumAnyCondition = p => p.Any(x => true);
            AnyCondition = ((MethodCallExpression)enumAnyCondition.Body).Method.GetGenericMethodDefinition();
        }

        public static MethodInfo Any { get; }

        public static MethodInfo AnyCondition { get; }

        public static MethodInfo DefaultIfEmpty { get; }
    }
}