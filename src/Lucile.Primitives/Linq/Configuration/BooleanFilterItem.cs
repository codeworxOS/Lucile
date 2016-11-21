using System;
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

        protected override Expression BuildExpression(ParameterExpression parameter)
        {
            var value = Value.GetExpression(parameter);

            switch (Operator)
            {
                case BooleanOperator.IsTrue:
                    return value;

                case BooleanOperator.IsFalse:
                    return Expression.Not(value);
            }

            throw new NotSupportedException();
        }
    }
}