using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Lucile.ViewModel;

namespace System
{
    public static class TaskExtensions
    {
        public static async void InvokeAsync(this Task task)
        {
            Exception exception = null;

            try
            {
                await task.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            if (exception != null)
            {
                var context = ViewModelOperations.DefaultSynchronizationContext;
                Action<object> action = p =>
                {
                    var ex = (Exception)p;
                    ExceptionDispatchInfo.Capture(ex).Throw();
                };

                if (context != null)
                {
                    context.Post(new Threading.SendOrPostCallback(action), exception);
                }
                else
                {
                    action(exception);
                }
            }
        }
    }
}