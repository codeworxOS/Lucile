using System;
using System.Linq.Expressions;

namespace Lucile.Linq.Configuration
{
    public class PathValueExpression : ValueExpression
    {
        public PathValueExpression(string path)
        {
            Path = path;
        }

        public string Path { get; }

        public override Expression GetExpression(ParameterExpression parameter)
        {
            Expression result = parameter;
            foreach (var item in Path.Split('.'))
            {
                try
                {
                    result = Expression.Property(result, item);
                }
                catch (ArgumentException)
                {
                    throw new InvalidPathException(result.Type, item);
                }
            }

            return result;
        }
    }
}