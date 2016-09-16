using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeworx.Dynamic.Interceptor
{

    public class InterceptionContext : InterceptionContextBase<IMethodInterceptor>
    {
        private bool bodyExecuted;

        public InterceptionContext(object instance, string memberName, Delegate methodBody, params object[] arguments)
            : base(instance, memberName, methodBody, arguments)
        {

        }


        public void Execute()
        {
            List<IMethodInterceptor> before = null, after = null, instead = null;

            before = this.GetInterceptors(InterceptionMode.BeforeBody);
            instead = this.GetInterceptors(InterceptionMode.InsteadOfBody);
            after = this.GetInterceptors(InterceptionMode.AfterBody);

            if (before != null) {
                foreach (var item in before) {
                    item.Execute(this);
                }
            }

            if (instead != null) {
                instead.First().Execute(this);
            } else {
                var result = ExecuteBody();
                this.SetResult(result);
            }

            if (after != null) {
                foreach (var item in after) {
                    item.Execute(this);
                }
            }
        }

        public object ExecuteBody()
        {
            var result = this.MethodBody.DynamicInvoke(this.Arguments);
            bodyExecuted = true;
            return result;
        }

        public override bool BodyExecuted
        {
            get { return this.bodyExecuted; }
        }
    }
}
