using System;
using System.ServiceModel;
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

        public void RaiseSampleFault()
        {
            throw new FaultException<SampleFault>(new SampleFault { Text = "SampleFault" });
        }
    }
}