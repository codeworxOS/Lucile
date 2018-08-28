using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Lucile.Dynamic.Test.DependencyInjection
{
    [ServiceContract]
    public interface ISyncService
    {
        [OperationContract]
        string SyncMethodWithResult(string param1, int param2);

        [OperationContract]
        void SyncVoidMethod(string param1, int param2);
    }
}