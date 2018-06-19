using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Lucile.Dynamic.Interceptor
{
    public class InterceptionContext : InterceptionContextBase<IMethodInterceptor>
    {
        private static readonly ConcurrentDictionary<Type, Func<Delegate, object[], object>> _invokeCache = new ConcurrentDictionary<Type, Func<Delegate, object[], object>>();

        private bool _bodyExecuted;

        public InterceptionContext(object instance, string memberName, Delegate methodBody, params object[] arguments)
            : base(instance, memberName, methodBody, arguments)
        {
        }

        public override bool BodyExecuted
        {
            get { return this._bodyExecuted; }
        }

        public void Execute()
        {
            List<IMethodInterceptor> before = null, after = null, instead = null;

            before = this.GetInterceptors(InterceptionMode.BeforeBody);
            instead = this.GetInterceptors(InterceptionMode.InsteadOfBody);
            after = this.GetInterceptors(InterceptionMode.AfterBody);

            if (before != null)
            {
                foreach (var item in before)
                {
                    item.Execute(this);
                }
            }

            if (instead != null)
            {
                instead.First().Execute(this);
            }
            else
            {
                var result = ExecuteBody();
                this.SetResult(result);
            }

            if (after != null)
            {
                foreach (var item in after)
                {
                    item.Execute(this);
                }
            }
        }

        public object ExecuteBody()
        {
            var delegateType = this.MethodBody.GetType();
            var func = _invokeCache.GetOrAdd(delegateType, CreateInvokeFunc);

            var result = func.Invoke(this.MethodBody, this.Arguments);
            ////var result = this.MethodBody.DynamicInvoke(this.Arguments);
            _bodyExecuted = true;
            return result;
        }

        private static Func<Delegate, object[], object> CreateInvokeFunc(Type delegateType)
        {
            var isAction = delegateType.Name.StartsWith("Action", StringComparison.Ordinal);

            var genericArguments = delegateType.GetGenericArguments();

            var param = Expression.Parameter(typeof(Delegate));
            var param2 = Expression.Parameter(typeof(object[]));

            var invokeParams = Enumerable.Range(0, genericArguments.Length - (isAction ? 0 : 1)).Select((i) => Expression.Convert(
                                                                    Expression.ArrayIndex(param2, Expression.Constant(i)),
                                                                    genericArguments[i])).ToList();

            Expression body = Expression.Call(Expression.Convert(param, delegateType), delegateType.GetMethod("Invoke"), invokeParams);

            if (isAction)
            {
                body = Expression.Block(body, Expression.Constant(null, typeof(object)));
            }
            else
            {
                body = Expression.Convert(body, typeof(object));
            }

            return Expression.Lambda<Func<Delegate, object[], object>>(body, param, param2).Compile();
        }
    }
}