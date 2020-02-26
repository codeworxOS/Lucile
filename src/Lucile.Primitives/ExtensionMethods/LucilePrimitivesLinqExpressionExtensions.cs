using Lucile.Linq.Configuration;

namespace System.Linq.Expressions
{
    public static class LucilePrimitivesLinqExpressionExtensions
    {
        public static bool IsConstantValueAccessor(this Expression expression, out object value)
        {
            if (expression is MemberExpression memberExpression &&
                memberExpression.Expression is ConstantExpression constantExpression &&
                constantExpression.Value is IConstantValueAccessor constantValueAccessor)
            {
                value = constantValueAccessor.GetValue();
                return true;
            }

            value = null;
            return false;
        }
    }
}
