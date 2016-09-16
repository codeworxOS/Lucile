using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Codeworx.Dynamic.Interceptor
{
    public class AsyncInterceptionContext : InterceptionContextBase<IAsyncMethodInterceptor>
    {
        private bool bodyExecuted;

        public AsyncInterceptionContext(object instance, string memberName, Delegate methodBody, params object[] arguments)
            : base(instance, memberName, methodBody, arguments)
        {
            var returnType = methodBody.Method.ReturnType;

            this.HasResult = typeof(Task).IsAssignableFrom(returnType) && returnType.IsGenericType;
        }

        public bool HasResult { get; private set; }

        public async Task<TResult> ExecuteAsync<TResult>()
        {
            List<IAsyncMethodInterceptor> before = null, after = null, instead = null;

            before = this.GetInterceptors(InterceptionMode.BeforeBody);
            instead = this.GetInterceptors(InterceptionMode.InsteadOfBody);
            after = this.GetInterceptors(InterceptionMode.AfterBody);

            if (before != null) {
                foreach (var item in before) {
                    await item.ExecuteAsync(this);
                }
            }

            if (instead != null) {
                await instead.First().ExecuteAsync(this);
            } else {
                if (HasResult) {
                    var result = await ExecuteBodyAsync<TResult>();
                    this.SetResult(result);
                } else {
                    await ExecuteBodyAsync();
                }
            }

            if (after != null) {
                foreach (var item in after) {
                    await item.ExecuteAsync(this);
                }
            }
            return (TResult)this.Result;
        }

        public async Task<object> ExecuteBodyAsync()
        {
            var task = (Task)this.MethodBody.DynamicInvoke(this.Arguments);
            await task;
            bodyExecuted = true;
            if (HasResult) {
                var param = Expression.Parameter(typeof(Task));
                var returnType = this.MethodBody.Method.ReturnType;
                Expression body = Expression.Property(Expression.Convert(param, returnType), "Result");
                if (body.Type.IsValueType) {
                    body = Expression.Convert(body, typeof(object));
                }
                var func = Expression.Lambda<Func<Task, object>>(body, param).Compile();
                return func(task);
            }
            return null;
        }

        public async Task<TResult> ExecuteBodyAsync<TResult>()
        {
            var result = await (Task<TResult>)this.MethodBody.DynamicInvoke(this.Arguments);
            bodyExecuted = true;
            return result;
        }

        public override bool BodyExecuted
        {
            get { return bodyExecuted; }
        }
    }
}
