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
        }

        public static MethodInfo DefaultIfEmpty { get; }
    }
}