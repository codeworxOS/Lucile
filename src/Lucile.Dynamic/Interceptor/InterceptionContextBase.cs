using System;
using System.Reflection;

namespace Lucile.Dynamic.Interceptor
{
    public abstract class InterceptionContextBase
    {
        public InterceptionContextBase(object instance, string memberName, Delegate methodBody, params object[] arguments)
        {
            this.MemberName = memberName;
            this.Instance = instance;
            this.Arguments = arguments;

            this.MethodBody = methodBody;

            this.ReturnType = this.MethodBody.GetMethodInfo().ReturnType;
            this.IsVoid = this.ReturnType == typeof(void);
        }

        public object[] Arguments { get; private set; }

        public abstract bool BodyExecuted { get; }

        public object Instance { get; private set; }

        public bool IsVoid { get; private set; }

        public string MemberName { get; private set; }

        public object Result { get; private set; }

        public Type ReturnType { get; private set; }

        protected Delegate MethodBody { get; private set; }

        public void SetResult(object value)
        {
            this.Result = value;
        }
    }
}