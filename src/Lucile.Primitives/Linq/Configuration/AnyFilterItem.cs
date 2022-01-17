using System;
using System.Collections.Generic;
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
            Operator = AnyOperator.Any;
        }

        public AnyFilterItem(PathValueExpression path, FilterItem item, AnyOperator anyOperator)
        {
            Path = path;
            Filter = item;
            Operator = anyOperator;
        }

        public FilterItem Filter { get; }

        public AnyOperator Operator { get; }

        public PathValueExpression Path { get; }

        public override IEnumerable<ValueExpression> GetValueExpressions()
        {
            yield return Path;
            foreach (var item in Filter.GetValueExpressions())
            {
                yield return item;
            }
        }

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

            Expression result = null;

            if (Filter != null)
            {
                var lambda = Expression.Lambda(Filter.GetExpression(param), param);

                result = Expression.Call(EnumerableInfo.AnyCondition.MakeGenericMethod(elementType), pathExpression, lambda);
            }
            else
            {
                result = Expression.Call(EnumerableInfo.Any.MakeGenericMethod(elementType), pathExpression);
            }

            if (Operator == AnyOperator.NotAny)
            {
                result = Expression.Not(result);
            }

            return result;
        }
    }
}