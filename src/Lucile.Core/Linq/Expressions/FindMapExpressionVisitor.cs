using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Lucile.Mapper;

namespace Lucile.Linq.Expressions
{
    public class FindMapExpressionVisitor : ExpressionVisitor
    {
        private static List<MethodInfo> _mapMethods;

        private List<MethodCallExpression> _mapCalls;

        static FindMapExpressionVisitor()
        {
            _mapMethods = typeof(MappingExtensions).GetMethods(BindingFlags.Public | BindingFlags.Static).Where(p => p.Name == nameof(MappingExtensions.Map)).ToList();
        }

        public FindMapExpressionVisitor()
        {
            _mapCalls = new List<MethodCallExpression>();
            MapCalls = new ReadOnlyCollection<MethodCallExpression>(this._mapCalls);
        }

        public ReadOnlyCollection<MethodCallExpression> MapCalls { get; private set; }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.IsGenericMethod && _mapMethods.Contains(node.Method.GetGenericMethodDefinition()))
            {
                this._mapCalls.Add(node);
            }

            return base.VisitMethodCall(node);
        }
    }
}