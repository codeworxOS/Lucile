using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Lucile.Dynamic.Interceptor
{
    public abstract class InterceptionContextBase
    {
        private static readonly ConcurrentDictionary<Type, Func<Delegate, object[], object>> _invokeCache = new ConcurrentDictionary<Type, Func<Delegate, object[], object>>();

        public InterceptionContextBase(object instance, string memberName, Delegate methodBody, params object[] arguments)
        {
            this.MemberName = memberName;
            this.Instance = instance;
            this.Arguments = arguments;

            this.MethodBody = methodBody;

            this.ReturnType = this.MethodBody.GetMethodInfo().ReturnType;
            this.IsVoid = this.ReturnType == typeof(void);
        }

        public object[] Arguments { get; }

        public abstract bool BodyExecuted { get; }

        public object Instance { get; }

        public bool IsVoid { get; }

        public string MemberName { get; }

        public object Result { get; private set; }

        public Type ReturnType { get; }

        protected Delegate MethodBody { get; }

        public Func<Delegate, object[], object> GetCallDelegate()
        {
            var delegateType = this.MethodBody.GetType();
            return _invokeCache.GetOrAdd(delegateType, CreateInvokeFunc);
        }

        public void SetResult(object value)
        {
            this.Result = value;
        }

        protected Delegate GetTargetDelegate<TTarget>(TTarget target)
        {
            var targetMethod = typeof(TTarget).GetMethod(MemberName, MethodBody.Method.GetParameters().Select(p => p.ParameterType).ToArray());

            if (targetMethod == null)
            {
                throw new MissingMethodException($"No matching method {MemberName} found on type {typeof(TTarget)}");
            }

            var targetDelegate = Delegate.CreateDelegate(MethodBody.GetType(), target, targetMethod);
            return targetDelegate;
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