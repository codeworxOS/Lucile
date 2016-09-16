using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeworx.Input
{
    public interface IAsyncCommand : ICommandBase
    {
        Task ExecuteAsync(object parameter);

        Task<bool> CanExecuteAsync(object parameter);

        bool IsExecuting { get; }
    }
}
