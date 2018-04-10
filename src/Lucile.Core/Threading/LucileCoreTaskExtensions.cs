using Lucile.Core.Threading;

namespace System.Threading.Tasks
{
    public static class LucileCoreTaskExtensions
    {
#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void

        public static async void InvokeAsync(this Task task)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            try
            {
                await task.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                ExceptionHandler.RaiseAsyncVoidException(ex);
            }
        }
    }
}