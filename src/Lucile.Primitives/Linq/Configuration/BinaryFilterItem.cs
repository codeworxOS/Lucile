using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Lucile.Linq.Configuration
{
    public abstract class BinaryFilterItem : FilterItem
    {
        public BinaryFilterItem(ValueExpression left, ValueExpression right)
        {
            Left = left;
            Right = right;
        }

        public ValueExpression Left { get; }

        public ValueExpression Right { get; }

        protected abstract Expression BuildBinaryExpression(Expression leftExpression, Expression rightExpression);

        protected override Expression BuildExpression(ParameterExpression parameter)
        {
            var left = Left.GetExpression(parameter);
            var info = left.Type.GetTypeInfo();
            switch (GetNullableOperation())
            {
                case NullableOperation.IsNull:
                    if (info.IsValueType && Nullable.GetUnderlyingType(left.Type) == null)
                    {
                        throw new NotSupportedException("IsNull Operator are only supported for Nullable primitive types!");
                    }

                    return Expression.Equal(left, Expression.Constant(null, left.Type));

                case NullableOperation.IsNotNull:
                    if (info.IsValueType && Nullable.GetUnderlyingType(left.Type) == null)
                    {
                        throw new NotSupportedException("IsNotNull Operator are only supported for Nullable primitive types!");
                    }

                    return Expression.NotEqual(left, Expression.Constant(null, left.Type));
            }

            var right = Right.GetExpression(parameter);

            return BuildBinaryExpression(left, right);
        }

        protected abstract NullableOperation GetNullableOperation();
    }
}