using System;
using System.Linq;
using System.Linq.Expressions;
using Lucile.Reflection;

namespace Lucile.Linq.Configuration
{
    public class AnyFilterItem : FilterItem
    {
        public AnyFilterItem(PathValueExpression path, FilterItem item)
        {
            Path = path;
            Filter = item;
        }

        public FilterItem Filter { get; }

        public PathValueExpression Path { get; }

        protected override Expression BuildExpression(ParameterExpression parameter)
        {
            var pathExpression = Path.GetExpression(parameter);

            var resultType = pathExpression.Type;
            var elementType = resultType.GenericTypeArguments?.FirstOrDefault();

            if (elementType == null)
            {
                throw new NotSupportedException("The given Path is not a collection.");
            }

            var param = Expression.Parameter(elementType);

            var lambda = Expression.Lambda(Filter.GetExpression(param), param);

            return Expression.Call(EnumerableInfo.AnyCondition.MakeGenericMethod(elementType), pathExpression, lambda);
        }
    }
}