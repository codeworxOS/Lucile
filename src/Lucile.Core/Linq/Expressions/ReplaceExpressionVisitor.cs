using System.Linq.Expressions;

namespace Lucile.Linq.Expressions
{
    public class ReplaceExpressionVisitor : ExpressionVisitor
    {
        public ReplaceExpressionVisitor(Expression searchExpression, Expression replaceExpression)
        {
            this.SearchExpression = searchExpression;
            this.ReplaceExpression = replaceExpression;
        }

        public Expression ReplaceExpression { get; private set; }

        public Expression SearchExpression { get; private set; }

        public override Expression Visit(Expression node)
        {
            if (node == SearchExpression)
            {
                return this.ReplaceExpression;
            }

            return base.Visit(node);
        }
    }
}