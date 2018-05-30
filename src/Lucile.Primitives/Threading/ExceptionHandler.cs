using System;

namespace Lucile.Core.Threading
{
    public static class ExceptionHandler
    {
        public static event EventHandler<ExceptionEventArgs> AsyncVoidException;

        internal static void RaiseAsyncVoidException(Exception exception)
        {
            AsyncVoidException?.Invoke(null, new ExceptionEventArgs(exception));
        }
    }
}