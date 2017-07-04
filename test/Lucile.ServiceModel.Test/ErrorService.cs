using System;
using System.Threading.Tasks;

namespace Lucile.ServiceModel.Test
{
    public class ErrorService : IErrorService
    {
        public Task RaiseAsyncErrorAsync()
        {
            throw new ArgumentException("Whatever");
        }

        public void RaiseError()
        {
            throw new ArgumentException("Whatever");
        }
    }
}