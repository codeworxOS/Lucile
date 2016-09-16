using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text;

namespace Codeworx.Data
{
    [DataContract(IsReference=true)]
    public class IncludePath : EntityTypeObject
    {
        public IncludePath()
        {

        }

        [DataMember]
        public string Path { get; set; }

        public static IncludePath Create<TEntity>(System.Linq.Expressions.Expression<Func<TEntity, object>> path)
        {
            var include = new IncludePath();

            var body = path.Body;

            string pathString = null;

            if (!TryGetPath(body, out pathString)) {
                throw new ArgumentException("The given path expression is not valid", "path");
            }

            include.Path = pathString;
            include.EntityType = typeof(TEntity);

            return include;
        }

        public static bool TryGetPath(Expression expression, out string path)
        {
            path = null;
            Expression expression2 = expression.RemoveConvert();
            MemberExpression memberExpression = expression2 as MemberExpression;
            MethodCallExpression methodCallExpression = expression2 as MethodCallExpression;
            if (memberExpression != null) {
                string name = memberExpression.Member.Name;
                string text;
                if (!TryGetPath(memberExpression.Expression, out text)) {
                    return false;
                }
                path = ((text == null) ? name : (text + "." + name));
            } else {
                if (methodCallExpression != null) {
                    if (methodCallExpression.Method.Name == "Select" && methodCallExpression.Arguments.Count == 2) {
                        string text2;
                        if (!TryGetPath(methodCallExpression.Arguments[0], out text2)) {
                            return false;
                        }
                        if (text2 != null) {
                            LambdaExpression lambdaExpression = methodCallExpression.Arguments[1] as LambdaExpression;
                            if (lambdaExpression != null) {
                                string text3;
                                if (!TryGetPath(lambdaExpression.Body, out text3)) {
                                    return false;
                                }
                                if (text3 != null) {
                                    path = text2 + "." + text3;
                                    return true;
                                }
                            }
                        }
                    }
                    return false;
                }
            }
            return true;
        }
    }
}
