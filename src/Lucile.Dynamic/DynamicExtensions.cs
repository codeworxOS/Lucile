using System.Linq.Expressions;
using System.Reflection;
using Lucile.Dynamic;

namespace System
{
    public static class DynamicExtensions
    {
        public static DynamicVoidDeclaration Void<TInterface>(this DynamicObjectBase baseObject, Expression<Func<TInterface, Action>> voidExpression)
        {
            return new DynamicVoidDeclaration() { DynamicObject = baseObject, CallingMemberName = GetMemberName(voidExpression) };
        }

        public static DynamicVoidDeclaration<TArg1> Void<TInterface, TArg1>(this DynamicObjectBase baseObject, Expression<Func<TInterface, Action<TArg1>>> voidExpression)
        {
            return new DynamicVoidDeclaration<TArg1>() { DynamicObject = baseObject, CallingMemberName = GetMemberName(voidExpression) };
        }

        private static string GetMemberName(LambdaExpression expression)
        {
            var unex = expression.Body as UnaryExpression;
            if (unex != null && unex.Operand is MethodCallExpression)
            {
                var memex = unex.Operand as MethodCallExpression;
                if (memex != null && memex.Arguments.Count == 3 && memex.Arguments[2] is ConstantExpression && ((ConstantExpression)memex.Arguments[2]).Value is MethodInfo)
                {
                    return ((MethodInfo)((ConstantExpression)memex.Arguments[2]).Value).Name;
                }

                if (memex != null && memex.Object is ConstantExpression && ((ConstantExpression)memex.Object).Value is MethodInfo)
                {
                    return ((MethodInfo)((ConstantExpression)memex.Object).Value).Name;
                }
            }

            return null;
        }
    }
}