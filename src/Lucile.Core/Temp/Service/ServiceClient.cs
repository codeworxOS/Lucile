using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Text;

namespace Codeworx.Service
{
    public class ServiceClient<TCallback> : ServiceClient
        where TCallback : class, ISubscriberCallback
    {
        public ServiceClient(string name)
            : base(name)
        {
            this.Callback = CallbackEnvironment.GetCallback<TCallback>();
        }

        public TCallback Callback { get; private set; }
    }

    public abstract class ServiceClient
    {
        private Collection<string> subscriptions;

        private object subscriptionLocker = new object();

        public ServiceClient(string name)
        {
            if (OperationContext.Current != null) {
                var props = OperationContext.Current.IncomingMessageProperties;
                var endpointProperty = props[System.ServiceModel.Channels.RemoteEndpointMessageProperty.Name] as System.ServiceModel.Channels.RemoteEndpointMessageProperty;
                if (endpointProperty != null) {
                    IPAddress address;
                    if (IPAddress.TryParse(endpointProperty.Address, out address)) {
                        this.IPAddress = address;
                    }
                }
            }
            this.SessionId = CallbackEnvironment.SessionId;
            this.Name = name;

            this.Connected = DateTime.UtcNow;
            this.subscriptions = new Collection<string>();
            this.Subscriptions = new ReadOnlyCollection<string>(this.subscriptions);
        }

        public virtual ClientInfo ToClientInfo()
        {
            return new ClientInfo {
                Address = this.IPAddress,
                Connected = this.Connected,
                DisplayName = this.Name
            };
        }

        public void Subscribe(string method)
        {
            lock (subscriptionLocker) {
                if (!this.subscriptions.Contains(method))
                    this.subscriptions.Add(method);
            }
        }

        public void UnSubscribe(string method)
        {
            lock (subscriptionLocker) {
                this.subscriptions.Remove(method);
            }
        }

        public bool IsSubscribed(string method)
        {
            lock (subscriptionLocker) {
                return this.subscriptions.Contains(method);
            }
        }

        public ReadOnlyCollection<string> Subscriptions { get; private set; }

        public string Name { get; private set; }

        public string SessionId { get; set; }

        public IPAddress IPAddress { get; set; }

        public DateTime Connected { get; private set; }

        public override int GetHashCode()
        {
            return (this.Name == null ? 0 : this.Name.GetHashCode());
        }

        public override bool Equals(object obj)
        {
            var client = obj as ServiceClient;
            if (client == null)
                return false;

            return object.Equals(this.Name, client.Name);
        }
    }
}
