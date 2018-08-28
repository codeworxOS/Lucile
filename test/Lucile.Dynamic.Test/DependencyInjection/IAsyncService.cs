using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Lucile.Dynamic.Test.DependencyInjection
{
    [ServiceContract]
    public interface IAsyncService
    {
        [OperationContract]
        Task<string> AsyncMethodWithResult(string param1, int param2);

        [OperationContract]
        Task AsyncVoidMethod(string param1, int param2);

        [OperationContract]
        Task<bool> IsAliveAsync();
    }
}