using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Lucile.Linq.Expressions
{
    public class FindExpressionVisitor<TExpressionType> : ExpressionVisitor
        where TExpressionType : Expression
    {
        [ThreadStatic]
        private static List<TExpressionType> _foundExpressions;

        [ThreadStatic]
        private static Func<TExpressionType, bool> _filter;

        public FindExpressionVisitor()
        {
        }

        public IEnumerable<TExpressionType> Find(Expression node, Func<TExpressionType, bool> filter = null)
        {
            try
            {
                _filter = filter;
                _foundExpressions = new List<TExpressionType>();

                Visit(node);
                return _foundExpressions;
            }
            finally
            {
                _filter = null;
                _foundExpressions = null;
            }
        }

        public override Expression Visit(Expression node)
        {
            var found = node as TExpressionType;
            if (found != null && (_filter == null || _filter(found)))
            {
                _foundExpressions.Add(found);
            }

            return base.Visit(node);
        }
    }
}