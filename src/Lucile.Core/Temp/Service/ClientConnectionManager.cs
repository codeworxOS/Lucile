using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Codeworx.Service
{
    public class ClientConnectionManager<TCallback, TServiceClient>
        where TCallback : class, ISubscriberCallback
        where TServiceClient : ServiceClient<TCallback>
    {
        private object clientLocker = new object();

        private Dictionary<string, TServiceClient> clients;

        public ClientConnectionManager()
        {
            this.clients = new Dictionary<string, TServiceClient>();
        }

        public IEnumerable<TServiceClient> GetRegisteredClients()
        {
            lock (clientLocker) {
                return this.clients.Values.ToList();
            }
        }

        public TServiceClient GetCurrentClient()
        {
            TServiceClient client;
            lock (clientLocker) {
                this.clients.TryGetValue(CallbackEnvironment.SessionId, out client);
            }
            return client;
        }

        public void Register(TServiceClient client)
        {
            Register(client, false);
        }

        public void Register(TServiceClient client, bool forceReconnect)
        {
            TServiceClient oldClient = null;

            lock (clientLocker) {
                if (this.clients.ContainsValue(client)) {
                    oldClient = this.clients.Values.Single(p => p.Equals(client));
                }
            }

            if (oldClient != null) {
                Task.Factory.StartNew(() => DoCallback(oldClient.Callback, p => p.Ping()));
            }

            lock (clientLocker) {
                if (this.clients.ContainsValue(client)) {
                    if (forceReconnect) {
                        Unregister(oldClient.SessionId);
                        var callback = oldClient.Callback as ICommunicationObject;
                        if (callback != null) {
                            callback.Abort();
                        }
                    } else {
                        throw new ClientAlreadyRegisteredException(client.Name);
                    }
                }
                if (this.clients.ContainsKey(client.SessionId)) {
                    throw new ClientAlreadyRegisteredException(client.SessionId, "A client is already registered in this session. Try to unregister first.");
                }
                client.Subscribe("ClientConnected");
                client.Subscribe("ClientDisconnected");
                var communication = client.Callback as ICommunicationObject;
                if (communication != null) {
                    communication.Closed += Callback_Closed;
                    communication.Faulted += Callback_Closed;
                }

                if (OperationContext.Current != null) {
                    OperationContext.Current.Channel.Closed += Channel_Closed;
                }

                this.clients.Add(client.SessionId, client);
            }
            var info = client.ToClientInfo();
            RaiseClientConnected(client);
            Publish(p => p.ClientConnected(info));
        }

        private void Channel_Closed(object sender, EventArgs e)
        {
            ((IContextChannel)sender).Closed -= Channel_Closed;

            var sessionid = ((IContextChannel)sender).SessionId;
            Unregister(sessionid);
        }

        private void Callback_Closed(object sender, EventArgs e)
        {
            ((ICommunicationObject)sender).Closed -= Callback_Closed;
            ((ICommunicationObject)sender).Faulted -= Callback_Closed;

            string session;

            lock (clientLocker) {
                session = this.clients.Where(p => p.Value.Callback == sender).Select(p => p.Key).FirstOrDefault();
            }

            if (session != null) {
                Unregister(session);
            }
        }

        public void Unregister()
        {
            Unregister(CallbackEnvironment.SessionId);
        }

        private void Unregister(string sessionId)
        {
            TServiceClient client;

            lock (clientLocker) {
                if (this.clients.TryGetValue(sessionId, out client)) {
                    this.clients.Remove(sessionId);
                }
            }
            if (client != null) {
                var info = client.ToClientInfo();
                RaiseClientDisconnected(client);
                Publish(p => p.ClientDisconnected(info));
            }
        }

        protected virtual void RaiseClientConnected(ServiceClient client)
        {
            if (ClientConnected != null) {
                ClientConnected(this, new ServiceClientEventArgs(client));
            }
        }

        protected virtual void RaiseClientDisconnected(ServiceClient client)
        {
            if (ClientDisconnected != null) {
                ClientDisconnected(this, new ServiceClientEventArgs(client));
            }
        }

        public event ServiceClientEventHandler ClientConnected;

        public event ServiceClientEventHandler ClientDisconnected;

        public async void Publish(Expression<Action<TCallback>> callback)
        {
#if SILVERLIGHT || NET4
            await TaskEx.Run(() => {
#else
            await Task.Run(() => {
#endif
                var mce = callback.Body as MethodCallExpression;
                if (mce == null || mce.Object != callback.Parameters.First()) {
                    throw new NotSupportedException("Only direct Method calls to the parameter are allowed! e.g. [p => p.ClientConnection(info)]");
                }
                var methodName = mce.Method.Name;

                List<TServiceClient> subscribedClients = new List<TServiceClient>();
                lock (clientLocker) {
                    subscribedClients.AddRange(this.clients.Values.Where(p => p.IsSubscribed(methodName)));
                }

                var callbacks = subscribedClients.Select(p => p.Callback).ToList();

                var compiled = callback.Compile();

                Parallel.ForEach(callbacks, p => DoCallback(p, compiled));
            });
        }

        private void DoCallback(TCallback channel, Action<TCallback> callback)
        {
            try {
                callback(channel);
            } catch (TimeoutException) {
                string session;

                lock (clientLocker) {
                    session = this.clients.Where(p => p.Value.Callback == channel).Select(p => p.Key).FirstOrDefault();
                }

                if (session != null) {
                    Unregister(session);
                }
            } catch { };
        }
    }
}
