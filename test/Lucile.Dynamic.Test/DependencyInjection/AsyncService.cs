using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Lucile.Dynamic.Test.DependencyInjection
{
    public class AsyncService : IAsyncService
    {
        private readonly ScopedDependency _scopedDependency;

        public AsyncService(ScopedDependency scopedDependency)
        {
            this._scopedDependency = scopedDependency;
        }

        public async Task<string> AsyncMethodWithResult(string param1, int param2)
        {
            await Task.Yield();
            return param1;
        }

        public Task AsyncVoidMethod(string param1, int param2)
        {
            throw new AsyncServiceException(_scopedDependency);
        }

        public Task<bool> IsAliveAsync()
        {
            return Task.FromResult(true);
        }

        [Serializable]
        public class AsyncServiceException : Exception
        {
            public AsyncServiceException(ScopedDependency scopedDependency)
            {
                ScopedDependency = scopedDependency;
            }

            public AsyncServiceException(string message) : base(message)
            {
            }

            public AsyncServiceException(string message, Exception innerException) : base(message, innerException)
            {
            }

            protected AsyncServiceException(SerializationInfo info, StreamingContext context) : base(info, context)
            {
            }

            public ScopedDependency ScopedDependency { get; }
        }
    }
}