using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Codeworx.Service
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
        ConcurrencyMode = ConcurrencyMode.Multiple)]
    public abstract class SubscribableService<TCallback, TServiceClient> : ISubscribable
        where TServiceClient : ServiceClient<TCallback>
        where TCallback : class, ISubscriberCallback
    {
        protected ClientConnectionManager<TCallback, TServiceClient> ConnectionManager { get; private set; }

        public SubscribableService()
        {
            this.ConnectionManager = new ClientConnectionManager<TCallback, TServiceClient>();
        }

        protected abstract TServiceClient CreateServiceClient(string identifier);


        #region ISubscribable Members

#pragma warning disable 1998
        public async Task RegisterClientAsync(string identifier)
        {
            var client = CreateServiceClient(identifier);
            this.ConnectionManager.Register(client);
        }

        public async Task ForceRegisterClientAsync(string identifier)
        {
            var client = CreateServiceClient(identifier);
            this.ConnectionManager.Register(client, true);
        }
#pragma warning restore 1998

        public Task UnRegisterClientAsync()
        {
#if SILVERLIGHT || NET4
            return TaskEx.Run(() => {
#else
            return Task.Run(() => {
#endif
                this.ConnectionManager.Unregister();
            });
        }

        public Task SubscribeAsync(string method)
        {
#if SILVERLIGHT || NET4
            return TaskEx.Run(() => {
#else
            return Task.Run(() => {
#endif
                var client = this.ConnectionManager.GetCurrentClient();
                if (client == null) {

                }
                client.Subscribe(method);
            });
        }

        public Task UnSubscribeAsync(string method)
        {
#if SILVERLIGHT || NET4
            return TaskEx.Run(() => {
#else
            return Task.Run(() => {
#endif
                var client = this.ConnectionManager.GetCurrentClient();
                if (client == null) {

                }
                client.Subscribe(method);
            });
        }

        public Task<IEnumerable<ClientInfo>> GetClientsAsync()
        {
#if SILVERLIGHT || NET4
            return TaskEx.Run<IEnumerable<ClientInfo>>(() => {
#else
            return Task.Run<IEnumerable<ClientInfo>>(() => {
#endif
                return this.ConnectionManager.GetRegisteredClients().Select(p => p.ToClientInfo()).ToList();
            });
        }

        public async Task PingAsync()
        {
            // do nothing
#if SILVERLIGHT || NET4
            await TaskEx.Yield();
#else
            await Task.Yield();
#endif
        }

        #endregion
    }
}
