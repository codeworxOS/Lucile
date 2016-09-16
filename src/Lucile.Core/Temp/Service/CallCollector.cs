using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Codeworx.Core.Service;
using Codeworx.Threading;

namespace Codeworx.Service
{
    public class CallCollector
    {
        public class CollectorScope : IDisposable
        {

            private CallCollector collector;

            internal CollectorScope(CallCollector collector)
            {
                this.collector = collector;
            }

            public CallCollector CreateChildCollector()
            {
                return this.collector.CreateChildCollector();
            }

            public ServiceResult<TResult> Call<TService, TResult>(Expression<Func<TService, Task<TResult>>> call)
            {
                return this.collector.Call<TService, TResult>(call);
            }

            #region IDisposable
            private bool _disposed;

            protected virtual bool IsDisposed
            {
                get
                {
                    return _disposed;
                }
            }

            ~CollectorScope()
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
                    _disposed = true;

                    if (disposing)
                    {
                        this.collector.CollectingCompleted();
                    }
                    // Cleanup native resources here!
                }
            }
            #endregion
        }

        protected abstract class TaskResultDispatcher
        {
            public abstract void SetResult(object value);

            public abstract void SetCanceled();

            public abstract void SetException(Exception ex);
        }

        protected class TaskResultDispatcher<TResult> : TaskResultDispatcher
        {
            public TaskCompletionSource<TResult> TaskCompletion { get; private set; }

            public TaskResultDispatcher()
            {
                this.TaskCompletion = new TaskCompletionSource<TResult>();
            }

            public override void SetResult(object value)
            {
                this.TaskCompletion.SetResult((TResult)value);
            }

            public override void SetException(Exception ex)
            {
                this.TaskCompletion.TrySetException(ex);
            }

            public override void SetCanceled()
            {
                this.TaskCompletion.TrySetCanceled();
            }
        }

        private object resultsLocker = new object();

        private Task<IEnumerable<CallAggregationResult>> resultsTask;

        private IEnumerable<CallAggregationResult> results;

        private bool isCollectionCompleted;

        private bool isInitialized;

        private List<CallCollector> children;

        private object childrenLocker = new object();

        protected ConcurrentDictionary<Type, CallAggregationServiceDescription> Services { get; private set; }

        protected ConcurrentDictionary<CallAggregationCallDescription, TaskResultDispatcher> CallResults { get; private set; }

        public bool MeasureDuration { get; private set; }

        public CallCollector Parent { get; private set; }

        public bool IsCollecting { get; private set; }

        public CallCollector Root
        {
            get
            {
                var parent = this;
                while (parent.Parent != null)
                {
                    parent = parent.Parent;
                }
                return parent;
            }
        }

        public CancellationManager CancellationManager { get; private set; }

        private CancellationToken token;

        public CollectorScope StartCollecting()
        {
            if (isInitialized)
            {
                throw new InvalidOperationException("StartCollecting can only be called once per call Collector. Use child collectors instead.");
            }
            else
            {
                isInitialized = true;
            }
            if (Parent != null && Parent.isCollectionCompleted)
            {
                isInitialized = false;
                throw new InvalidOperationException("StartCollecting can only be called when the parent collector has not yet finished collecting.");
            }

            IsCollecting = true;
            return new CollectorScope(this);
        }

        public bool IsCollectingCompleted
        {
            get
            {
                lock (childrenLocker)
                {
                    return (isCollectionCompleted || !isInitialized) && children.All(p => p.IsCollectingCompleted);
                }
            }
        }

        public CallCollector(CancellationManager cancellationManager, bool measureDuration)
            : this(cancellationManager)
        {
            this.MeasureDuration = measureDuration;
        }

        public CallCollector(CancellationManager cancellationManager)
            : this()
        {
            this.CancellationManager = cancellationManager;
            this.token = this.CancellationManager.GetToken();
            this.Services = new ConcurrentDictionary<Type, CallAggregationServiceDescription>();
            this.CallResults = new ConcurrentDictionary<CallAggregationCallDescription, TaskResultDispatcher>();
        }

        protected CallCollector(CallCollector parent)
            : this()
        {
            this.Parent = parent;
        }

        private CallCollector()
        {
            this.children = new List<CallCollector>();
        }

        private ServiceResult<TResult> Call<TService, TResult>(Expression<Func<TService, Task<TResult>>> call)
        {
            if (isCollectionCompleted)
                throw new InvalidOperationException("Do not use the Call method after the collector has been marked as Completed.");

            var methodCall = call.Body as MethodCallExpression;
            if (methodCall == null)
            {
                throw new ArgumentOutOfRangeException("call", "Only expression with a method call body are allowed!");
            }

            var methodName = methodCall.Method.Name;
            var parameters = methodCall.Arguments.Select(p =>
                                    Expression.Lambda<Func<TService, object>>(
                                        Expression.Convert(p, typeof(object)),
                                        call.Parameters)
                                    .Compile()(default(TService))).ToList();

            var service = Root.Services.GetOrAdd(typeof(TService), p => new CallAggregationServiceDescription(p));
            var callDescription = service.GetOrAddCall(methodName, parameters);
            callDescription.MeasureDuration = Root.MeasureDuration;

            var completion = (TaskResultDispatcher<TResult>)Root.CallResults.GetOrAdd(callDescription, p => new TaskResultDispatcher<TResult>());

            return new ServiceResult<TResult>(this, completion.TaskCompletion.Task);

        }

        private CallCollector CreateChildCollector()
        {
            if (isCollectionCompleted)
                throw new InvalidOperationException("Do not use the CreateChildCollector method after the collector has been marked as Completed.");

            lock (childrenLocker)
            {
                var child = new CallCollector(this);
                this.children.Add(child);
                return child;
            }
        }

        private void CollectingCompleted()
        {
            isCollectionCompleted = true;
            IsCollecting = false;
            Root.CallAggregationServiceIfCollectionCompleted();
        }

        protected async void CallAggregationServiceIfCollectionCompleted()
        {
            if (IsCollectingCompleted)
            {
                try
                {
                    await GetResultsAsync();
                }
                catch { }
            }
        }

        public async Task<IEnumerable<CallAggregationResult>> GetResultsAsync()
        {
            IEnumerable<CallAggregationResult> result;

            if (!Services.Values.Any())
                return Enumerable.Empty<CallAggregationResult>();

            try
            {
                Task<IEnumerable<CallAggregationResult>> awaitable;

                lock (resultsLocker)
                {
                    if (this.results != null)
                        return this.results;

                    if (this.resultsTask == null)
                    {
                        this.resultsTask = GetResultsFromServiceAsync(token);
                    }

                    awaitable = this.resultsTask;
                }

                result = await awaitable;

                lock (resultsLocker)
                {
                    this.results = result;
                    this.resultsTask = null;
                }
            }
            catch (Exception ex)
            {
                foreach (var item in this.CallResults.Values)
                {
                    item.SetException(ex);
                }
                throw;
            }

            return result;
        }

        private bool CheckCancelled(CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                CallResults.Values.ToList().ForEach(p => p.SetCanceled());
                return true;
            }
            return false;
        }

        private async Task<IEnumerable<CallAggregationResult>> GetResultsFromServiceAsync(CancellationToken token)
        {
            if (CheckCancelled(token))
                return null;

            ICallAggregatonService aggService = null;

            try
            {
                aggService = await ServiceContext.Current.GetServiceAsync<ICallAggregatonService>(token);

                if (CheckCancelled(token))
                    return null;

                if (aggService != null)
                {
                    var result = await aggService.GetResultsAsync(this.Services.Values);

                    var joined = (from r in result
                                  join s in CallResults on r.CallId equals s.Key.CallId
                                  select new { ResultDispatcher = s.Value, Result = r }).ToList();

                    if (CheckCancelled(token))
                        return null;

                    foreach (var item in joined)
                    {
                        item.ResultDispatcher.SetResult(item.Result.Value);
                    }

                    return result;
                }
            }
            finally
            {
                var disp = aggService as IDisposable;
                if (disp != null)
                {
                    disp.Dispose();
                }
            }
            return null;
        }
    }
}
