using System.Linq.Expressions;

namespace Lucile.Linq.Configuration
{
    public class NumericConstantValue : ValueExpression
    {
        public NumericConstantValue(decimal value)
        {
            Value = value;
        }

        public decimal Value { get; }

        public override Expression GetExpression(ParameterExpression parameter)
        {
            return Expression.Constant(Value, typeof(decimal));
        }
    }
}