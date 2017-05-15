using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Lucile.Linq.Configuration
{
    public class StringBinaryFilterItem : BinaryFilterItem
    {
        private static readonly MethodInfo _containsMethod;
        private static readonly MethodInfo _endsWithMethod;
        private static readonly MethodInfo _startsWithMethod;

        static StringBinaryFilterItem()
        {
            Expression<Func<string, bool>> func = p => p.Contains("test");
            _containsMethod = ((MethodCallExpression)func.Body).Method;

            func = p => p.StartsWith("test", StringComparison.OrdinalIgnoreCase);
            _startsWithMethod = ((MethodCallExpression)func.Body).Method;

            func = p => p.EndsWith("test", StringComparison.OrdinalIgnoreCase);
            _endsWithMethod = ((MethodCallExpression)func.Body).Method;
        }

        public StringBinaryFilterItem(ValueExpression left, ValueExpression right, StringOperator stringOperator)
            : base(left, right)
        {
            Operator = stringOperator;
        }

        public StringOperator Operator { get; }

        protected override Expression BuildBinaryExpression(Expression leftExpression, Expression rightExpression)
        {
            switch (Operator)
            {
                case StringOperator.Equal:
                    return Expression.Equal(leftExpression, rightExpression);

                case StringOperator.NotEqual:
                    return Expression.NotEqual(leftExpression, rightExpression);

                case StringOperator.Contains:
                    return Expression.Call(leftExpression, _containsMethod, rightExpression);

                case StringOperator.StartsWith:
                    return Expression.Call(leftExpression, _startsWithMethod, rightExpression, Expression.Constant(StringComparison.OrdinalIgnoreCase, typeof(StringComparison)));

                case StringOperator.EndsWith:
                    return Expression.Call(leftExpression, _endsWithMethod, rightExpression, Expression.Constant(StringComparison.OrdinalIgnoreCase, typeof(StringComparison)));
            }

            throw new NotSupportedException();
        }

        protected override NullableOperation GetNullableOperation()
        {
            if (this.Operator == StringOperator.IsNull)
            {
                return NullableOperation.IsNull;
            }
            else if (this.Operator == StringOperator.IsNotNull)
            {
                return NullableOperation.IsNotNull;
            }

            return NullableOperation.None;
        }
    }
}