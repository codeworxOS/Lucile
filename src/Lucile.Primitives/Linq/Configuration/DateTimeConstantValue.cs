using System;
using System.Linq.Expressions;

namespace Lucile.Linq.Configuration
{
    public class DateTimeConstantValue : ValueExpression
    {
        public DateTimeConstantValue(DateTime value)
        {
            Value = value;
        }

        public DateTime Value { get; }

        public override Expression GetExpression(ParameterExpression parameter)
        {
            return Expression.Constant(Value, typeof(DateTime));
        }
    }
}