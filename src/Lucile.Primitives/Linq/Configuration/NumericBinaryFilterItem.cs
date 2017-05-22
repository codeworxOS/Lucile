using System;
using System.Linq.Expressions;
using System.Reflection;

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
                if (leftExpression is ConstantExpression)
                {
                    leftExpression = ConvertExpression((ConstantExpression)leftExpression, rightType);
                }
                else if (rightExpression is ConstantExpression)
                {
                    rightExpression = ConvertExpression((ConstantExpression)rightExpression, leftType);
                }
                else
                {
                    rightExpression = Expression.Convert(rightExpression, leftType);
                }
            }

            return base.BuildBinaryExpression(leftExpression, rightExpression);
        }

        private static Expression ConvertExpression(ConstantExpression constant, Type targetType)
        {
            if (targetType.GetTypeInfo().IsEnum)
            {
                var intValue = Convert.ToInt32(constant.Value);
                var enumValue = Enum.ToObject(targetType, intValue);
                return Expression.Constant(enumValue, targetType);
            }

            return Expression.Convert(constant, targetType);
        }
    }
}