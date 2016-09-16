using System;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Security.Principal;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using Codeworx.Core.Service;
using Codeworx.Dynamic;
using Codeworx.Dynamic.Interceptor;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace Codeworx.Service
{
    public abstract class ChannelProxy<TChannel> : DynamicObjectBase, IDisposable, IConnectionStateObject where TChannel : class
    {
#if !SILVERLIGHT
        private IPrincipal principal;
#endif
        private bool firstConnection;

        public static int RetryCount { get; set; }

        public static TimeSpan RetryOffset { get; set; }

        private object reconnectTaskLocker = new object();

        private TaskCompletionSource<bool> reconnectTaskCompletion;

        static ChannelProxy()
        {
            RetryCount = -1;
            RetryOffset = TimeSpan.FromSeconds(5);
        }

        protected virtual int GetRetryCount()
        {
            return RetryCount;
        }

        protected virtual TimeSpan GetRetryOffset()
        {
            return RetryOffset;
        }

        private ConnectionState state;

        public ConnectionState State
        {
            get { return state; }
            protected set
            {
                if (state != value)
                {
                    var oldValue = state;
                    state = value;
                    OnStateChanged(oldValue, state);
                }
            }
        }


        public string SessionId { get; set; }

        public Func<Exception, bool> RetryCondition { get; set; }

        public ChannelProxy()
        {
#if !SILVERLIGHT
            this.principal = Thread.CurrentPrincipal;
#endif
            this.reconnectTaskCompletion = new TaskCompletionSource<bool>();
            this.State = ConnectionState.Pending;
            firstConnection = true;
            lock (isConnectingLocker)
            {
                ensureServiceTaskCompletion = new TaskCompletionSource<bool>();
            }
            SessionId = Guid.NewGuid().ToString();
        }

        public event EventHandler<ConnectionStateChangedEventArgs> ConnectionStateChanged;

        protected virtual void OnStateChanged(ConnectionState oldState, ConnectionState newState)
        {
            switch (newState)
            {

                case ConnectionState.Connected:
                    lock (reconnectTaskLocker)
                    {
                        if (this.reconnectTaskCompletion.Task.IsCompleted)
                        {
                            this.reconnectTaskCompletion = new TaskCompletionSource<bool>();
                        }
                        this.reconnectTaskCompletion.SetResult(true);
                    }
                    break;
                case ConnectionState.Pending:
                    lock (reconnectTaskLocker)
                    {
                        if (this.reconnectTaskCompletion.Task.IsCompleted)
                        {
                            this.reconnectTaskCompletion = new TaskCompletionSource<bool>();
                        }
                    }
                    break;
                case ConnectionState.Checking:
                    break;
                case ConnectionState.None:
                case ConnectionState.Disconnected:
                default:
                    lock (reconnectTaskLocker)
                    {
                        if (this.reconnectTaskCompletion.Task.IsCompleted)
                        {
                            this.reconnectTaskCompletion = new TaskCompletionSource<bool>();
                        }
                        this.reconnectTaskCompletion.SetResult(false);
                    }
                    break;
            }

            if (ConnectionStateChanged != null)
            {
                ConnectionStateChanged(this, new ConnectionStateChangedEventArgs(oldState, newState));
            }
        }

        protected virtual void OnChannel(TChannel channel)
        {
            bool success = false;

            if (channel != null)
            {
                var comm = channel as ICommunicationObject;
                if (comm != null)
                {
                    comm.Closed -= Channel_Closed;
                    comm.Faulted -= Channel_Closed;
                    comm.Closed += Channel_Closed;
                    comm.Faulted += Channel_Closed;
                }
                this.State = ConnectionState.Connected;
                success = true;
            }
            else {
                this.State = ConnectionState.Disconnected;
            }

            lock (isConnectingLocker)
            {
                if (firstConnection)
                {
                    firstConnection = false;
                    var completion = this.ensureServiceTaskCompletion;
                    ensureServiceTaskCompletion = null;
                    completion.SetResult(success);
                }
            }
        }

        public void CheckConnection()
        {
            switch (state)
            {
                case ConnectionState.Connected:
                case ConnectionState.Disconnected:
                    this.State = ConnectionState.Checking;
                    break;
            }
        }

        private void Channel_Closed(object sender, EventArgs e)
        {
            ((ICommunicationObject)sender).Closed -= Channel_Closed;
            ((ICommunicationObject)sender).Faulted -= Channel_Closed;

            this.State = ConnectionState.Disconnected;
        }

        public void Reset()
        {
            var channel = ((IDynamicProxy)this).GetProxyTarget<TChannel>();
            var comm = channel as ICommunicationObject;
            var disp = channel as IDisposable;
            if (comm != null)
            {
                comm.Closed -= Channel_Closed;
                comm.Faulted -= Channel_Closed;
            }
            if (disp != null)
            {
                disp.Dispose();
            }
            this.State = ConnectionState.Disconnected;
        }

        [MethodInterceptor(Codeworx.Dynamic.Interceptor.InterceptionMode.BeforeBody)]
        protected void MethodInterceptor(InterceptionContext context)
        {
            if (context.MemberName == "SetProxyTarget" && context.Arguments.First() is TChannel)
            {
                var oldProxy = ((IDynamicProxy)this).GetProxyTarget<TChannel>();
                var comm = oldProxy as ICommunicationObject;
                if (comm != null)
                {
                    comm.Closed -= Channel_Closed;
                    comm.Faulted -= Channel_Closed;
                }

                if (oldProxy != null)
                {
                    var disp = oldProxy as IDisposable;
                    if (disp != null)
                    {
                        Task.Factory.StartNew(() => disp.Dispose(), TaskCreationOptions.LongRunning);
                    }
                }

                OnChannel((TChannel)context.Arguments.First());
            }
#if !SILVERLIGHT
            if (OperationContext.Current != null && OperationContext.Current.Host == null && Thread.CurrentPrincipal != this.principal)
            {
                Thread.CurrentPrincipal = this.principal;
            }
#endif
        }

        [MethodInterceptor(Dynamic.Interceptor.InterceptionMode.InsteadOfBody)]
        protected async virtual Task ProxyMethodInterceptor(AsyncInterceptionContext context)
        {
#if !SILVERLIGHT
            var oldCallback = CallContext.LogicalGetData(CallbackEnvironment.CallContextKey);
            var oldSession = CallContext.LogicalGetData(CallbackEnvironment.CallContextSessionIdKey);
            CallContext.LogicalSetData(CallbackEnvironment.CallContextKey, this);
            CallContext.LogicalSetData(CallbackEnvironment.CallContextSessionIdKey, this.SessionId);
#endif
            int retrys = 0;

            try
            {
                while (GetRetryCount() == -1 || retrys <= GetRetryCount())
                {
                    try
                    {
                        if (await EnsureService(context))
                        {
                            var result = await context.ExecuteBodyAsync();
                            context.SetResult(result);
                            return;
                        }
                        else {
                            retrys++;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex is TargetInvocationException)
                        {
                            ex = ex.InnerException;
                        }
                        if (RetryCondition != null && RetryCondition(ex))
                        {
                            retrys++;
                            this.State = ConnectionState.Disconnected;
                        }
                        else {
#if (SILVERLIGHT || NET4)
                            throw ex;
#else
                                ExceptionDispatchInfo.Capture(ex).Throw();
#endif
                        }
                    }
                }
                if (context.HasResult && context.ReturnType.GetGenericArguments().First().IsValueType)
                {
                    context.SetResult(Activator.CreateInstance(context.ReturnType.GetGenericArguments().First()));
                }
            }
            finally
            {
#if !SILVERLIGHT
                CallContext.LogicalSetData(CallbackEnvironment.CallContextSessionIdKey, oldSession);
                CallContext.LogicalSetData(CallbackEnvironment.CallContextKey, oldCallback);
#endif
            }
        }

        private object isConnectingLocker = new object();

        private TaskCompletionSource<bool> ensureServiceTaskCompletion;

        private async Task<bool> EnsureService(AsyncInterceptionContext context)
        {
            if (this.State == ConnectionState.Connected || this.State == ConnectionState.Checking)
                return true;

            Task<bool> task = null;
            lock (isConnectingLocker)
            {
                if (ensureServiceTaskCompletion != null)
                {
                    task = ensureServiceTaskCompletion.Task;
                }
                else {
                    ensureServiceTaskCompletion = new TaskCompletionSource<bool>();
                }
            }

            if (task != null)
            {
                return await task;
            }
            else {
                this.State = ConnectionState.Pending;
#if SILVERLIGHT || NET4
                await TaskEx.Delay(GetRetryOffset());
#else
                await Task.Delay(GetRetryOffset());
#endif
                var tokens = context.Arguments.OfType<CancellationToken>();
                CancellationToken token = default(CancellationToken);
                if (tokens.Any())
                {
                    token = tokens.First();
                    if (token.IsCancellationRequested)
                        return false;
                }
                var reConnected = this.reconnectTaskCompletion.Task;

                try
                {
                    await ServiceContext.Current.GetServiceAsync<TChannel>(this, token);
                }
                catch
                {
                    this.State = ConnectionState.Disconnected;
                }
                var result = await reConnected;

                lock (isConnectingLocker)
                {
                    var completion = this.ensureServiceTaskCompletion;
                    ensureServiceTaskCompletion = null;
                    completion.SetResult(result);
                }

                return result;

            }
        }

        #region IDisposable Members

        private bool _disposed;

        protected virtual bool IsDisposed
        {
            get
            {
                return _disposed;
            }
        }

        ~ChannelProxy()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    var dynproxy = this as IDynamicProxy;
                    if (dynproxy != null)
                    {
                        var proxyDisposable = dynproxy.GetProxyTarget<TChannel>() as IDisposable;
                        var proxyComm = dynproxy.GetProxyTarget<TChannel>() as ICommunicationObject;
                        if (proxyComm != null)
                        {
                            proxyComm.Closed -= Channel_Closed;
                            proxyComm.Faulted -= Channel_Closed;
                        }

                        if (proxyDisposable != null)
                        {
                            try
                            {
                                proxyDisposable.Dispose();
                            }
                            catch { /* shit happens */ }
                        }
                    }
                }
                // Cleanup native resources here!
                _disposed = true;
            }
        }

        #endregion
    }
}
