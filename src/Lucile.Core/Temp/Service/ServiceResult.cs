using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeworx.Service
{
    public class ServiceResult<T>
    {
        private Task<T> result;

        private CallCollector collector;

        internal ServiceResult(CallCollector collector, Task<T> result)
        {
            this.collector = collector;
            this.result = result;
        }

        public Task<T> GetResultAsync() {
            if (collector.IsCollecting) {
                throw new InvalidOperationException("The Result can only be requested after collection has been completed. Maybe you forgot to call Dispose on the CollectorScope.");
            }
            return result;   
        }
    }
}
