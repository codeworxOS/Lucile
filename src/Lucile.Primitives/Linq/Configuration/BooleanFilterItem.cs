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

                case BooleanOperator.IsNull:
                    return Expression.Equal(value, Expression.Constant(null, typeof(bool?)));

                case BooleanOperator.IsNotNull:
                    return Expression.NotEqual(value, Expression.Constant(null, typeof(bool?)));
            }

            throw new NotSupportedException();
        }
    }
}