using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Lucile.Dynamic.Test.DependencyInjection
{
    public class SyncService : ISyncService
    {
        private readonly ScopedDependency _scopedDependency;

        public SyncService(ScopedDependency scopedDependency)
        {
            this._scopedDependency = scopedDependency;
        }

        public string SyncMethodWithResult(string param1, int param2)
        {
            return param1;
        }

        public void SyncVoidMethod(string param1, int param2)
        {
            throw new SyncServiceException(_scopedDependency);
        }

        [Serializable]
        public class SyncServiceException : Exception
        {
            public SyncServiceException(ScopedDependency scopedDependency)
            {
                ScopedDependency = scopedDependency;
            }

            public SyncServiceException(string message) : base(message)
            {
            }

            public SyncServiceException(string message, Exception innerException) : base(message, innerException)
            {
            }

            protected SyncServiceException(SerializationInfo info, StreamingContext context) : base(info, context)
            {
            }

            public ScopedDependency ScopedDependency { get; }
        }
    }
}