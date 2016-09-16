using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace Codeworx.Service
{
    public interface ISubscriberCallback
    {
        [OperationContract(IsOneWay = true)]
        void ClientConnected(ClientInfo info);

        [OperationContract(IsOneWay = true)]
        void ClientDisconnected(ClientInfo info);

        [OperationContract(IsOneWay = true)]
        void Ping();
    }
}
