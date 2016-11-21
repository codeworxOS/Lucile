using System.Linq.Expressions;

namespace Lucile.Linq.Configuration
{
    public class ConstantValueExpression<TValue> : ValueExpression
    {
        public ConstantValueExpression(TValue value)
        {
            Value = value;
        }

        public TValue Value { get; }

        public override Expression GetExpression(ParameterExpression parameter)
        {
            return Expression.Constant(Value, typeof(TValue));
        }
    }
}