using System;
using System.Linq.Expressions;

namespace Lucile.Linq.Configuration
{
    public class GuidConstantValue : ValueExpression
    {
        public GuidConstantValue(Guid value)
        {
            Value = value;
        }

        public Guid Value { get; }

        public override Expression GetExpression(ParameterExpression parameter)
        {
            return Expression.Constant(Value, typeof(Guid));
        }
    }
}