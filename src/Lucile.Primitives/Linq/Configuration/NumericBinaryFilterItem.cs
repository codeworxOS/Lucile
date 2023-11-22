using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace Lucile.Linq.Configuration
{
    public class NumericBinaryFilterItem : RelationalFilterItem
    {
        private static readonly ConcurrentDictionary<Type, Func<object, Expression>> _enumConstantValueFactoryCache;
        private static readonly MethodInfo _getExpressionMethod;

        static NumericBinaryFilterItem()
        {
            _enumConstantValueFactoryCache = new ConcurrentDictionary<Type, Func<object, Expression>>();
            Expression<Func<Expression>> exp = () => GetEnumExpression<object>(null);
            _getExpressionMethod = ((MethodCallExpression)exp.Body).Method.GetGenericMethodDefinition();
        }

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
                if (leftExpression.IsConstantValueAccessor(out var valueLeft))
                {
                    leftExpression = ConvertExpression(leftExpression, valueLeft, rightType);
                }
                else if (rightExpression.IsConstantValueAccessor(out var valueRight))
                {
                    rightExpression = ConvertExpression(rightExpression, valueRight, leftType);
                }
                else
                {
                    rightExpression = Expression.Convert(rightExpression, leftType);
                }
            }

            return base.BuildBinaryExpression(leftExpression, rightExpression);
        }

        private static Expression ConvertExpression(Expression constant, object value, Type targetType)
        {
            if (targetType.GetTypeInfo().IsEnum)
            {
                var intValue = Convert.ToInt32(value);
                var enumValue = Enum.ToObject(targetType, intValue);

                var factory = _enumConstantValueFactoryCache.GetOrAdd(targetType, CreateEnumConstantValueFactory);
                var result = factory(enumValue);
                return result;
            }

            return Expression.Convert(constant, targetType);
        }

        private static Func<object, Expression> CreateEnumConstantValueFactory(Type arg)
        {
            var param = Expression.Parameter(typeof(object), "p");
            return Expression.Lambda<Func<object, Expression>>(
                            Expression.Call(
                                _getExpressionMethod.MakeGenericMethod(arg),
                                Expression.Convert(param, arg)),
                            param).Compile();
        }

        private static Expression GetEnumExpression<T>(T enumValue)
        {
            var constantValue = new ConstantValueExpression<T>(enumValue);
            return constantValue.GetExpression(null);
        }
    }
}