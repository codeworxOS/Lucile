using System;
using System.Threading;
using System.Threading.Tasks;

namespace Lucile.Threading
{
    public interface IViewOperations
    {
        bool CheckAccess();

        Task<TResult> ExecuteAsync<TResult>(Func<TResult> func, CancellationToken token = default(CancellationToken));

        Task ExecuteAsync(Action action, CancellationToken token = default(CancellationToken));
    }
}