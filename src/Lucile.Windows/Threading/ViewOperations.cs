using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Lucile.Threading;

namespace Lucile.Windows.Threading
{
    public class ViewOperations : IViewOperations
    {
        private readonly Dispatcher _dispatcher;

        public ViewOperations(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public bool CheckAccess()
        {
            return _dispatcher.CheckAccess();
        }

        public async Task<TResult> ExecuteAsync<TResult>(Func<TResult> func, CancellationToken token = default(CancellationToken))
        {
            return await _dispatcher.InvokeAsync<TResult>(func, DispatcherPriority.Normal, token);
        }

        public async Task ExecuteAsync(Action action, CancellationToken token = default(CancellationToken))
        {
            await _dispatcher.InvokeAsync(action, DispatcherPriority.Normal, token);
        }
    }
}