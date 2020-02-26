using System.Linq.Expressions;

namespace Lucile.Linq.Configuration
{
    public class ConstantValueExpression<TValue> : ValueExpression
    {
        public ConstantValueExpression(TValue value)
        {
            Value = value;
            Accessor = new ValueAccessor(value);
        }

        public TValue Value { get; }

        public ValueAccessor Accessor { get; }

        public override Expression GetExpression(ParameterExpression parameter)
        {
            var constant = Expression.Constant(Accessor, typeof(ValueAccessor));
            return Expression.Property(constant, "Value");
        }

        public class ValueAccessor : IConstantValueAccessor
        {
            public ValueAccessor(TValue value)
            {
                Value = value;
            }

            public TValue Value { get; }

            public object GetValue() => Value;
        }
    }
}