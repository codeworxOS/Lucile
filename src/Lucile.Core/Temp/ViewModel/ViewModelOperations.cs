using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Codeworx.ViewModel;

namespace Codeworx.Core.ViewModel
{
    public class ViewModelOperations
    {
        private static SynchronizationContext defaultSynchronizationContext;

        private static Thread defaultSynchronizationContextThread;

        public static bool CheckAccess()
        {
            if (DefaultSynchronizationContext != null) {
                return defaultSynchronizationContextThread == Thread.CurrentThread;
            } else {
                return false;
            }
        }

        public static Task<TResult> ExecuteAsync<TResult>(Func<TResult> func, CancellationToken token = default(CancellationToken))
        {
            Task<TResult> task = null;
            var context = DefaultSynchronizationContext;
            if (context == null) {
#if(NET4 || SILVERLIGHT)
                task = TaskEx.Run(func, token);
#else
                task = Task.Run(func, token);
#endif
            } else {
                var continuation = new TaskCompletionSource<TResult>();
                task = continuation.Task;
                Action<object> contextAction = p => {
                    var tuple = (Tuple<TaskCompletionSource<TResult>, CancellationToken>)p;
                    var c = tuple.Item1;
                    var t = tuple.Item2;
                    if (t.IsCancellationRequested) {
                        c.SetCanceled();
                        return;
                    }
                    try {
                        var result = func();
                        if (t.IsCancellationRequested) {
                            c.SetCanceled();
                            return;
                        }
                        c.SetResult(result);
                    } catch (Exception ex) {
                        c.SetException(ex);
                    }
                };

                context.Post(new SendOrPostCallback(contextAction), new Tuple<TaskCompletionSource<TResult>, CancellationToken>(continuation, token));
            }
            return task;
        }

        public static Task ExecuteAsync(Action action, CancellationToken token = default(CancellationToken))
        {
            return ExecuteAsync<bool>(() => { action(); return true; }, token);
        }

        public static SynchronizationContext DefaultSynchronizationContext
        {
            get
            {
                if (defaultSynchronizationContext == null) {
                    DefaultSynchronizationContext = SynchronizationContext.Current;
                }
                return defaultSynchronizationContext;
            }
            set
            {
                if (value == null) {
                    defaultSynchronizationContextThread = null;
                } else {
                    value.Send(new SendOrPostCallback(p => defaultSynchronizationContextThread = Thread.CurrentThread), null);
                }
                defaultSynchronizationContext = value;
            }
        }
    }
}
