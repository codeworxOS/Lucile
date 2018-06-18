using System;
using System.Collections.Generic;

namespace Lucile.Dynamic.Interceptor
{
    public abstract class InterceptionContextBase<TInterceptor> : InterceptionContextBase
        where TInterceptor : IMethodInterceptorBase
    {
        private Dictionary<InterceptionMode, List<TInterceptor>> _interceptors;

        public InterceptionContextBase(object instance, string memberName, Delegate methodBody, params object[] arguments)
            : base(instance, memberName, methodBody, arguments)
        {
            this._interceptors = new Dictionary<InterceptionMode, List<TInterceptor>>();
        }

        public void RegisterInterceptor(TInterceptor interceptor)
        {
            if (!_interceptors.ContainsKey(interceptor.InterceptionMode))
            {
                _interceptors.Add(interceptor.InterceptionMode, new List<TInterceptor>());
            }

            var value = _interceptors[interceptor.InterceptionMode];

            if (interceptor.InterceptionMode == InterceptionMode.InsteadOfBody && value.Count > 0)
            {
                throw new ArgumentException("Only one InsteadOfBody interceptor can be registered.", nameof(interceptor));
            }

            value.Add(interceptor);
        }

        protected List<TInterceptor> GetInterceptors(InterceptionMode mode)
        {
            List<TInterceptor> result = null;
            this._interceptors.TryGetValue(mode, out result);

            return result;
        }
    }
}