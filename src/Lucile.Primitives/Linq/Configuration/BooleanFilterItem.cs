using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Lucile.Linq.Configuration
{
    public class BooleanFilterItem : FilterItem
    {
        public BooleanFilterItem(ValueExpression value, BooleanOperator compareOperator)
        {
            Value = value;
            Operator = compareOperator;
        }

        public BooleanOperator Operator { get; }

        public ValueExpression Value { get; }

        public override IEnumerable<ValueExpression> GetValueExpressions()
        {
            yield return Value;
        }

        protected override Expression BuildExpression(ParameterExpression parameter)
        {
            var value = Value.GetExpression(parameter);

            switch (Operator)
            {
                case BooleanOperator.IsTrue:
                    return Expression.Equal(value, Expression.Constant(true, value.Type));

                case BooleanOperator.IsFalse:
                    return Expression.Equal(value, Expression.Constant(false, value.Type));

                case BooleanOperator.IsNull:
                    return Expression.Equal(value, Expression.Constant(null, value.Type));

                case BooleanOperator.IsNotNull:
                    return Expression.NotEqual(value, Expression.Constant(null, value.Type));
            }

            throw new NotSupportedException();
        }
    }
}