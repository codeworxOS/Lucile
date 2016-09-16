using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeworx.Service
{
    public delegate void ServiceClientEventHandler(object sender, ServiceClientEventArgs args);

    public class ServiceClientEventArgs : EventArgs
    {
        public ServiceClientEventArgs(ServiceClient client)
        {
            this.Client = client;
        }

        public ServiceClient Client { get; private set; }
    }
}
