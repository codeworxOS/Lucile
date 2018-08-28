using System;
using System.Collections.Generic;
using System.Linq;

namespace Lucile.Dynamic.Interceptor
{
    public class InterceptionContext : InterceptionContextBase<IMethodInterceptor>
    {
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
            var func = GetCallDelegate();
            var result = func.Invoke(this.MethodBody, this.Arguments);
            _bodyExecuted = true;
            return result;
        }

        public object ExecuteBodyOn<TTarget>(TTarget target)
        {
            var targetDelegate = GetTargetDelegate<TTarget>();
            var result = targetDelegate.Invoke(target, this.Arguments);
            _bodyExecuted = true;
            return result;
        }
    }
}