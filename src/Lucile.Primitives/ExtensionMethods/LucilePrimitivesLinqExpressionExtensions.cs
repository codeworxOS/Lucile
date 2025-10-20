using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;
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

            var returnType = body.Type;

            if (body is NewExpression newExpression)
            {
                return ProcessNewExpression(newExpression, parameter);
            }
            else if (body is MemberInitExpression memberInit)
            {
                return ProcessMemberInitExpression(memberInit, parameter);
            }
            else
            {
                var visitorInit = new FindExpressionVisitor<MemberInitExpression>();

                var foundInit = visitorInit.Find(body, p => p.Type == returnType).FirstOrDefault();

                if (foundInit != null)
                {
                    return ProcessMemberInitExpression(foundInit, parameter);
                }

                var visitorNew = new FindExpressionVisitor<NewExpression>();

                var foundNew = visitorNew.Find(body, p => p.Type == returnType).FirstOrDefault();

                if (foundNew != null)
                {
                    return ProcessNewExpression(foundNew, parameter);
                }
            }

            return Enumerable.Empty<KeyValuePair<PropertyInfo, LambdaExpression>>();
        }

        private static IEnumerable<KeyValuePair<PropertyInfo, LambdaExpression>> ProcessMemberInitExpression(MemberInitExpression memberInit, ParameterExpression parameter)
        {
            foreach (var binding in memberInit.Bindings.OfType<MemberAssignment>())
            {
                if (binding.Member is PropertyInfo property)
                {
                    yield return new KeyValuePair<PropertyInfo, LambdaExpression>(property, Expression.Lambda(binding.Expression, parameter));
                }
            }
        }

        private static IEnumerable<KeyValuePair<PropertyInfo, LambdaExpression>> ProcessNewExpression(NewExpression newExpression, ParameterExpression parameter)
        {
            for (int i = 0; i < newExpression.Members.Count; i++)
            {
                if (newExpression.Members[i] is PropertyInfo property)
                {
                    yield return new KeyValuePair<PropertyInfo, LambdaExpression>(property, Expression.Lambda(newExpression.Arguments[i], parameter));
                }
            }
        }

        private class FindExpressionVisitor<TExpressionType> : ExpressionVisitor
        where TExpressionType : Expression
        {
            [ThreadStatic]
            private static List<TExpressionType> _foundExpressions;

            [ThreadStatic]
            private static Func<TExpressionType, bool> _filter;

            public FindExpressionVisitor()
            {
            }

            public IEnumerable<TExpressionType> Find(Expression node, Func<TExpressionType, bool> filter = null)
            {
                try
                {
                    _filter = filter;
                    _foundExpressions = new List<TExpressionType>();

                    Visit(node);
                    return _foundExpressions;
                }
                finally
                {
                    _filter = null;
                    _foundExpressions = null;
                }
            }

            public override Expression Visit(Expression node)
            {
                var found = node as TExpressionType;
                if (found != null && (_filter == null || _filter(found)))
                {
                    _foundExpressions.Add(found);
                }

                return base.Visit(node);
            }
        }
    }
}
