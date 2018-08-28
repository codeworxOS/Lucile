using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Lucile.Dynamic.Interceptor
{
    public class AsyncInterceptionContext : InterceptionContextBase<IAsyncMethodInterceptor>
    {
        private bool _bodyExecuted;

        public AsyncInterceptionContext(object instance, string memberName, Delegate methodBody, params object[] arguments)
            : base(instance, memberName, methodBody, arguments)
        {
            var returnType = methodBody.GetMethodInfo().ReturnType;

            this.HasResult = typeof(Task).IsAssignableFrom(returnType) && returnType.GetTypeInfo().IsGenericType;
        }

        public override bool BodyExecuted
        {
            get { return _bodyExecuted; }
        }

        public bool HasResult { get; private set; }

        public async Task<TResult> ExecuteAsync<TResult>()
        {
            List<IAsyncMethodInterceptor> before = null, after = null, instead = null;

            before = this.GetInterceptors(InterceptionMode.BeforeBody);
            instead = this.GetInterceptors(InterceptionMode.InsteadOfBody);
            after = this.GetInterceptors(InterceptionMode.AfterBody);

            if (before != null)
            {
                foreach (var item in before)
                {
                    await item.ExecuteAsync(this);
                }
            }

            if (instead != null)
            {
                await instead.First().ExecuteAsync(this);
            }
            else
            {
                if (HasResult)
                {
                    var result = await ExecuteBodyAsync<TResult>();
                    this.SetResult(result);
                }
                else
                {
                    await ExecuteBodyAsync();
                }
            }

            if (after != null)
            {
                foreach (var item in after)
                {
                    await item.ExecuteAsync(this);
                }
            }

            return (TResult)this.Result;
        }

        public async Task<object> ExecuteBodyAsync()
        {
            var callDelegate = GetCallDelegate();

            var task = (Task)callDelegate(this.MethodBody, this.Arguments);
            await task;
            _bodyExecuted = true;
            return GetTaskResult(task);
        }

        public async Task<TResult> ExecuteBodyAsync<TResult>()
        {
            var callDelegate = GetCallDelegate();

            var result = await (Task<TResult>)callDelegate(this.MethodBody, this.Arguments);
            _bodyExecuted = true;
            return result;
        }

        public async Task<object> ExecuteBodyOnAsync<TTarget>(TTarget target)
        {
            var targetDelegate = GetTargetDelegate<TTarget>();
            var task = (Task)targetDelegate(target, Arguments);
            await task;

            return GetTaskResult(task);
        }

        private object GetTaskResult(Task task)
        {
            if (HasResult)
            {
                var param = Expression.Parameter(typeof(Task));
                var returnType = this.MethodBody.GetMethodInfo().ReturnType;
                Expression body = Expression.Property(Expression.Convert(param, returnType), "Result");
                if (body.Type.GetTypeInfo().IsValueType)
                {
                    body = Expression.Convert(body, typeof(object));
                }

                var func = Expression.Lambda<Func<Task, object>>(body, param).Compile();
                return func(task);
            }

            return null;
        }
    }
}