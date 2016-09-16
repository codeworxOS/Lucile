using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using Codeworx.Core.ViewModel;

namespace System
{
    public static class TaskExtensions
    {
        public static async void InvokeAsync(this Task task)
        {
            Exception exception = null;

            try {
                await task.ConfigureAwait(false);
            } catch (Exception ex) {
                exception = ex;
            }

            if (exception != null) {
                var context = ViewModelOperations.DefaultSynchronizationContext;
                Action<object> action = p => {
                    var ex = (Exception)p;
#if(NET4 || SILVERLIGHT)
                    throw ex;
#else
                    ExceptionDispatchInfo.Capture(ex).Throw();
#endif
                };

                if (context != null) {
                    context.Post(new Threading.SendOrPostCallback(action), exception);
                } else {
                    action(exception);
                }
            }
        }
    }
}
