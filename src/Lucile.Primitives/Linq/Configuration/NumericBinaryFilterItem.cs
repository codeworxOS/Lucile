using System;
using System.Linq.Expressions;

namespace Lucile.Linq.Configuration
{
    public class NumericBinaryFilterItem : RelationalFilterItem
    {
        public NumericBinaryFilterItem(ValueExpression left, ValueExpression right, RelationalCompareOperator operatior)
            : base(left, right, operatior)
        {
        }

        protected override Expression BuildBinaryExpression(Expression leftExpression, Expression rightExpression)
        {
            var leftType = Nullable.GetUnderlyingType(leftExpression.Type) ?? leftExpression.Type;
            var rightType = Nullable.GetUnderlyingType(rightExpression.Type) ?? rightExpression.Type;

            if (leftType != rightType)
            {
                rightExpression = Expression.Convert(rightExpression, leftType);
            }

            return base.BuildBinaryExpression(leftExpression, rightExpression);
        }
    }
}