using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Codeworx.Service
{
    [ServiceContract(CallbackContract = typeof(ISubscriberCallback))]
    public interface ISubscribable
    {
        [OperationContract]
        Task RegisterClientAsync(string identifier);

        [OperationContract]
        Task ForceRegisterClientAsync(string identifier);

        [OperationContract]
        Task UnRegisterClientAsync();

        [OperationContract]
        Task SubscribeAsync(string method);

        [OperationContract]
        Task UnSubscribeAsync(string method);

        [OperationContract]
        Task<IEnumerable<ClientInfo>> GetClientsAsync();

        [OperationContract]
        Task PingAsync();
    }
}
