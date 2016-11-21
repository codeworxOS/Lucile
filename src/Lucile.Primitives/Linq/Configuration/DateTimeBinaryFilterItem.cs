using System;
using System.Linq.Expressions;

namespace Lucile.Linq.Configuration
{
    public class DateTimeBinaryFilterItem : BinaryFilterItem
    {
        public DateTimeBinaryFilterItem(ValueExpression left, ValueExpression right, RelationalCompareOperator compareOperator)
            : base(left, right)
        {
            Operator = compareOperator;
        }

        public RelationalCompareOperator Operator { get; }

        protected override Expression BuildBinaryExpression(Expression leftExpression, Expression rightExpression)
        {
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