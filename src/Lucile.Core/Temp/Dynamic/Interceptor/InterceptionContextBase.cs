using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeworx.Dynamic.Interceptor
{
    public abstract class InterceptionContextBase<TInterceptor> : InterceptionContextBase where TInterceptor : IMethodInterceptorBase
    {
        private Dictionary<InterceptionMode, List<TInterceptor>> interceptors;

        public InterceptionContextBase(object instance, string memberName, Delegate methodBody, params object[] arguments)
            : base(instance, memberName, methodBody, arguments)
        {
            this.interceptors = new Dictionary<InterceptionMode, List<TInterceptor>>();

        }

        public void RegisterInterceptor(TInterceptor interceptor)
        {
            if (!interceptors.ContainsKey(interceptor.InterceptionMode)) {
                interceptors.Add(interceptor.InterceptionMode, new List<TInterceptor>());
            }
            var value = interceptors[interceptor.InterceptionMode];

            if (interceptor.InterceptionMode == InterceptionMode.InsteadOfBody && value.Count > 0)
                throw new ArgumentException("Only one InsteadOfBody interceptor can be registered.", "interceptor");

            value.Add(interceptor);
        }


        protected List<TInterceptor> GetInterceptors(InterceptionMode mode)
        {
            List<TInterceptor> result = null;
            this.interceptors.TryGetValue(mode, out result);

            return result;
        }

    }

    public abstract class InterceptionContextBase
    {


        public InterceptionContextBase(object instance, string memberName, Delegate methodBody, params object[] arguments)
        {
            this.MemberName = memberName;
            this.Instance = instance;
            this.Arguments = arguments;

            this.MethodBody = methodBody;

            this.ReturnType = this.MethodBody.Method.ReturnType;
            this.IsVoid = this.ReturnType == typeof(void);
        }

        public void SetResult(object value)
        {
            this.Result = value;
        }

        public Type ReturnType { get; private set; }

        public bool IsVoid { get; private set; }

        public object[] Arguments { get; private set; }

        public string MemberName { get; private set; }

        public object Instance { get; private set; }

        protected Delegate MethodBody { get; private set; }

        public abstract bool BodyExecuted { get; }

        public object Result { get; private set; }
    }
}
