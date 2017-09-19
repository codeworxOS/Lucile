using System.Runtime.ExceptionServices;
using Lucile.ViewModel;

namespace System.Threading.Tasks
{
    public static class LucileCoreTaskExtensions
    {
#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void

        public static async void Invoke(this Task task)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
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