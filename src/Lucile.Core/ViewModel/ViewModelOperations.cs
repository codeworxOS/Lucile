using System;
using System.Threading;
using System.Threading.Tasks;

namespace Lucile.ViewModel
{
    public class ViewModelOperations
    {
        private static SynchronizationContext _defaultSynchronizationContext;
        private static Thread _defaultSynchronizationContextThread;

        public static SynchronizationContext DefaultSynchronizationContext
        {
            get
            {
                if (_defaultSynchronizationContext == null)
                {
                    DefaultSynchronizationContext = SynchronizationContext.Current;
                }

                return _defaultSynchronizationContext;
            }

            set
            {
                if (value == null)
                {
                    _defaultSynchronizationContextThread = null;
                }
                else
                {
                    value.Send(new SendOrPostCallback(p => _defaultSynchronizationContextThread = Thread.CurrentThread), null);
                }

                _defaultSynchronizationContext = value;
            }
        }

        public static bool CheckAccess()
        {
            if (DefaultSynchronizationContext != null)
            {
                return _defaultSynchronizationContextThread == Thread.CurrentThread;
            }
            else
            {
                return false;
            }
        }

        public static Task<TResult> ExecuteAsync<TResult>(Func<TResult> func, CancellationToken token = default(CancellationToken))
        {
            Task<TResult> task = null;
            var context = DefaultSynchronizationContext;
            if (context == null)
            {
                task = Task.Run(func, token);
            }
            else
            {
                var continuation = new TaskCompletionSource<TResult>();
                task = continuation.Task;
                Action<object> contextAction = p =>
                {
                    var tuple = (Tuple<TaskCompletionSource<TResult>, CancellationToken>)p;
                    var c = tuple.Item1;
                    var t = tuple.Item2;
                    if (t.IsCancellationRequested)
                    {
                        c.SetCanceled();
                        return;
                    }

                    try
                    {
                        var result = func();
                        if (t.IsCancellationRequested)
                        {
                            c.SetCanceled();
                            return;
                        }

                        c.SetResult(result);
                    }
                    catch (Exception ex)
                    {
                        c.SetException(ex);
                    }
                };

                context.Post(new SendOrPostCallback(contextAction), new Tuple<TaskCompletionSource<TResult>, CancellationToken>(continuation, token));
            }

            return task;
        }

        public static Task ExecuteAsync(Action action, CancellationToken token = default(CancellationToken))
        {
            return ExecuteAsync<bool>(
                () =>
                {
                    action();
                    return true;
                },
                token);
        }
    }
}