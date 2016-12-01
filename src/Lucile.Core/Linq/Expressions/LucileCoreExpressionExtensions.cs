using System.Collections.Generic;
using System.Reflection;
using Lucile.Linq.Expressions;

namespace System.Linq.Expressions
{
    public static class LucileCoreExpressionExtensions
    {
        public static IEnumerable<TExpression> Find<TExpression>(this Expression baseExpression, Func<TExpression, bool> filter)
            where TExpression : Expression
        {
            var visitor = new FindExpressionVisitor<TExpression>();
            return visitor.Find(baseExpression, filter);
        }

        public static PropertyInfo GetPropertyInfo(this LambdaExpression propertyExpression)
        {
            var body = propertyExpression.Body;

            var member = body.RemoveConvert() as MemberExpression;
            if (member == null || !(member.Expression is ParameterExpression))
            {
                throw new ArgumentException($"Only direct PropertyExpressions are supported.", nameof(propertyExpression));
            }

            return (PropertyInfo)member.Member;
        }

        public static string GetPropertyName(this LambdaExpression propertyExpression)
        {
            var propInfo = GetPropertyInfo(propertyExpression);
            return propInfo.Name;
        }

        public static Expression RemoveConvert(this Expression expression)
        {
            while (expression != null && (expression.NodeType == ExpressionType.Convert || expression.NodeType == ExpressionType.ConvertChecked))
            {
                expression = ((UnaryExpression)expression).Operand.RemoveConvert();
            }

            return expression;
        }

        public static TExpression Replace<TExpression>(this TExpression baseExpression, Expression searchExpression, Expression replaceExpression)
            where TExpression : Expression
        {
            var visitor = new ReplaceExpressionVisitor(searchExpression, replaceExpression);

            return (TExpression)visitor.Visit(baseExpression);
        }
    }
}