using System.Linq.Expressions;

namespace Lucile.Linq.Configuration
{
    public class StringConstantValue : ValueExpression
    {
        public StringConstantValue(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public override Expression GetExpression(ParameterExpression parameter)
        {
            return Expression.Constant(Value, typeof(string));
        }
    }
}