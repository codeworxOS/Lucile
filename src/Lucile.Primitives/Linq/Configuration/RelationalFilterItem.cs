﻿using System;
using System.Linq.Expressions;

namespace Lucile.Linq.Configuration
{
    public abstract class RelationalFilterItem : BinaryFilterItem
    {
        public RelationalFilterItem(ValueExpression left, ValueExpression right, RelationalCompareOperator operatior)
            : base(left, right)
        {
            this.Operator = operatior;
        }

        public RelationalCompareOperator Operator { get; }

        protected override Expression BuildBinaryExpression(Expression leftExpression, Expression rightExpression)
        {
            var nullable = Nullable.GetUnderlyingType(leftExpression.Type);
            Expression nullExpression = null;
            if (nullable != null)
            {
                nullExpression = Expression.Property(leftExpression, "HasValue");
                leftExpression = Expression.Property(leftExpression, "Value");
            }

            nullable = Nullable.GetUnderlyingType(rightExpression.Type);
            if (nullable != null)
            {
                if (nullExpression != null)
                {
                    nullExpression = Expression.AndAlso(nullExpression, Expression.Property(rightExpression, "HasValue"));
                }
                else
                {
                    nullExpression = Expression.Property(rightExpression, "HasValue");
                }

                rightExpression = Expression.Property(rightExpression, "Value");
            }

            Expression condition = null;

            switch (Operator)
            {
                case RelationalCompareOperator.Equal:
                    condition = Expression.Equal(leftExpression, rightExpression);
                    break;

                case RelationalCompareOperator.NotEqual:
                    condition = Expression.NotEqual(leftExpression, rightExpression);
                    break;

                case RelationalCompareOperator.GreaterThen:
                    condition = Expression.GreaterThan(leftExpression, rightExpression);
                    break;

                case RelationalCompareOperator.GreaterThenOrEqual:
                    condition = Expression.GreaterThanOrEqual(leftExpression, rightExpression);
                    break;

                case RelationalCompareOperator.LessThen:
                    condition = Expression.LessThan(leftExpression, rightExpression);
                    break;

                case RelationalCompareOperator.LessThenOrEqual:
                    condition = Expression.LessThanOrEqual(leftExpression, rightExpression);
                    break;
            }

            if (condition != null)
            {
                if (nullExpression == null)
                {
                    return condition;
                }
                else
                {
                    return Expression.AndAlso(nullExpression, condition);
                }
            }

            throw new NotSupportedException();
        }

        protected override NullableOperation GetNullableOperation()
        {
            if (this.Operator == RelationalCompareOperator.IsNull)
            {
                return NullableOperation.IsNull;
            }
            else if (this.Operator == RelationalCompareOperator.IsNotNull)
            {
                return NullableOperation.IsNotNull;
            }

            return NullableOperation.None;
        }
    }
}