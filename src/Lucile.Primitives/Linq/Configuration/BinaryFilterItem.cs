using System.Linq.Expressions;

namespace Lucile.Linq.Configuration
{
    public abstract class BinaryFilterItem : FilterItem
    {
        public BinaryFilterItem(ValueExpression left, ValueExpression right)
        {
            Left = left;
            Right = right;
        }

        public ValueExpression Left { get; }

        public ValueExpression Right { get; }

        protected abstract Expression BuildBinaryExpression(Expression leftExpression, Expression rightExpression);

        protected override Expression BuildExpression(ParameterExpression parameter)
        {
            var left = Left.GetExpression(parameter);
            var right = Right.GetExpression(parameter);

            return BuildBinaryExpression(left, right);
        }
    }
}