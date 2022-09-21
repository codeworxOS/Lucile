using System.Collections.Generic;
using System.Reflection;
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

        public static IEnumerable<KeyValuePair<PropertyInfo, LambdaExpression>> GetPropertyLambda(this LambdaExpression lambdaExpression)
        {
            ParameterExpression parameter = lambdaExpression.Parameters[0];
            var body = lambdaExpression.Body;

            if (body is NewExpression newExpression)
            {
                for (int i = 0; i < newExpression.Members.Count; i++)
                {
                    if (newExpression.Members[i] is PropertyInfo property)
                    {
                        yield return new KeyValuePair<PropertyInfo, LambdaExpression>(property, Expression.Lambda(newExpression.Arguments[i], parameter));
                    }
                }
            }
            else if (body is MemberInitExpression memberInit)
            {
                foreach (var binding in memberInit.Bindings.OfType<MemberAssignment>())
                {
                    if (binding.Member is PropertyInfo property)
                    {
                        yield return new KeyValuePair<PropertyInfo, LambdaExpression>(property, Expression.Lambda(binding.Expression, parameter));
                    }
                }
            }
        }
    }
}
