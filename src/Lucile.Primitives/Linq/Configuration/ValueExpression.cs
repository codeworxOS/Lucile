using System.Linq.Expressions;

namespace Lucile.Linq.Configuration
{
    public abstract class ValueExpression
    {
        public abstract Expression GetExpression(ParameterExpression parameter);
    }
}