using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;

namespace Lucile.Json
{
    internal class DelegatingMetaObject : DynamicMetaObject
    {
        private readonly IDynamicMetaObjectProvider _innerProvider;

        public DelegatingMetaObject(IDynamicMetaObjectProvider innerProvider, Expression expr, BindingRestrictions restrictions)
            : base(expr, restrictions)
        {
            this._innerProvider = innerProvider;
        }

        public DelegatingMetaObject(IDynamicMetaObjectProvider innerProvider, Expression expr, BindingRestrictions restrictions, object value)
            : base(expr, restrictions, value)
        {
            this._innerProvider = innerProvider;
        }

        public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
        {
            var innerMetaObject = _innerProvider.GetMetaObject(Expression.Constant(_innerProvider));
            return innerMetaObject.BindGetMember(binder);
        }

        public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args)
        {
            var innerMetaObject = _innerProvider.GetMetaObject(Expression.Constant(_innerProvider));
            return innerMetaObject.BindInvokeMember(binder, args);
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return base.GetDynamicMemberNames();
        }
    }
}