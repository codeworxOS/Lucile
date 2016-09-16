using System;
using System.Linq.Expressions;

namespace Codeworx.Input
{
    public class PropertyObserverStrategy : CanExecuteRequeryStrategy
    {
        public PropertyObserverStrategy(params Expression<Func<object>>[] properties)
        {

        }

        public PropertyObserverStrategy(Expression expression)
        {

        }
    }
}
