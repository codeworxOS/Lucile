using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Codeworx.Dynamic.Interceptor;
using Codeworx.Threading;

namespace Codeworx.Service
{
    public abstract class KeepAliveChannelProxy<TChannel> : ChannelProxy<TChannel> where TChannel : class, ISubscribable
    {
        static KeepAliveChannelProxy()
        {
            // default polling interval...
            PollingInterval = TimeSpan.FromMinutes(2);
        }

        public static TimeSpan PollingInterval { get; set; }

        private CancellationManager cancellationManager;

        public KeepAliveChannelProxy()
        {
            this.cancellationManager = new CancellationManager();
        }

        private async Task PollAsync(CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return;
            TChannel channel = this as TChannel;
            if (channel != null) {
                // Polling action
                try {
                    await channel.PingAsync();
                    if (this.State == ConnectionState.Checking) {
                        this.State = ConnectionState.Connected;
                    }
                } catch { }
            }
        }

        private async void Poll(object state)
        {
            await cancellationManager.Execute(PollAsync);
        }

        private object timerLocker = new object();
        private Timer pollingTimer = null;

        protected override int GetRetryCount()
        {
            return -1; // infinite
        }

        private void StartTimer(bool immediately = false)
        {
            if (disposed) {
                return;
            }

            lock (timerLocker) {
                if (this.pollingTimer != null) {
                    this.pollingTimer.Dispose();
                }
                this.pollingTimer = new Timer(
                    Poll,
                    null,
                    immediately ? TimeSpan.Zero : PollingInterval,
                    PollingInterval);
            }
        }

        private void StopTimer()
        {
            lock (timerLocker) {
                if (this.pollingTimer != null) {
                    this.pollingTimer.Dispose();
                    this.pollingTimer = null;
                }
            }
        }

        protected override void OnStateChanged(ConnectionState oldState, ConnectionState newState)
        {
            switch (newState) {
                case ConnectionState.Connected:
                    StartTimer();
                    break;
                case ConnectionState.Disconnected:
                    if (oldState == ConnectionState.Pending) {
                        StartTimer();
                    } else {
                        StartTimer(true);
                    }
                    break;
                case ConnectionState.Checking:
                    StartTimer(true);
                    break;
                case ConnectionState.Pending:
                    StopTimer();
                    break;
            }

            base.OnStateChanged(oldState, newState);
        }

        protected async override void OnChannel(TChannel channel)
        {
            var comm = channel as ICommunicationObject;
            try {

                string identity = ServiceSettings.ClientIdentity;
                if (Debugger.IsAttached)
                    await channel.ForceRegisterClientAsync(identity);
                else
                    await channel.RegisterClientAsync(identity);

                base.OnChannel(channel);
            } catch {
                base.OnChannel(null);
                if (comm != null && comm.State != CommunicationState.Opened) {
                    var disposable = channel as IDisposable;
                    if (disposable != null) {
                        try {
                            disposable.Dispose();
                        } catch { /* who cares*/ }
                    }
                }
            }
        }

        private bool disposed;

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed) {
                if (disposing) {
                    disposed = true;
                    lock (timerLocker) {
                        if (this.pollingTimer != null) {
                            this.pollingTimer.Dispose();
                        }
                    }
                }
                base.Dispose(disposing);
            }
        }
    }
}
