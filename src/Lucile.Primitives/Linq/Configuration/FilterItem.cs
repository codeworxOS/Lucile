using System;
using System.Linq.Expressions;

namespace Lucile.Linq.Configuration
{
    public abstract class FilterItem
    {
        public Expression GetExpression(ParameterExpression parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            var result = BuildExpression(parameter);

            return result;
        }

        protected abstract Expression BuildExpression(ParameterExpression parameter);
    }
}