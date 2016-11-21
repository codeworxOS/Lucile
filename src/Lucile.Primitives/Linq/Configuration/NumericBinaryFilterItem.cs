using System;
using System.Linq.Expressions;

namespace Lucile.Linq.Configuration
{
    public class NumericBinaryFilterItem : BinaryFilterItem
    {
        public NumericBinaryFilterItem(ValueExpression left, ValueExpression right, RelationalCompareOperator compareOperator)
            : base(left, right)
        {
            Operator = compareOperator;
        }

        public RelationalCompareOperator Operator { get; }

        protected override Expression BuildBinaryExpression(Expression leftExpression, Expression rightExpression)
        {
            var leftType = Nullable.GetUnderlyingType(leftExpression.Type) ?? leftExpression.Type;
            var rightType = Nullable.GetUnderlyingType(rightExpression.Type) ?? rightExpression.Type;

            if (leftType != rightType)
            {
                rightExpression = Expression.Convert(rightExpression, leftType);
            }

            switch (Operator)
            {
                case RelationalCompareOperator.Equal:
                    return Expression.Equal(leftExpression, rightExpression);

                case RelationalCompareOperator.NotEqual:
                    return Expression.NotEqual(leftExpression, rightExpression);

                case RelationalCompareOperator.GreaterThen:
                    return Expression.GreaterThan(leftExpression, rightExpression);

                case RelationalCompareOperator.GreaterThenOrEqual:
                    return Expression.GreaterThanOrEqual(leftExpression, rightExpression);

                case RelationalCompareOperator.LessThen:
                    return Expression.LessThan(leftExpression, rightExpression);

                case RelationalCompareOperator.LessThenOrEqual:
                    return Expression.LessThanOrEqual(leftExpression, rightExpression);
            }

            throw new NotSupportedException();
        }
    }
}