using System.Threading.Tasks;

namespace Lucile.Input
{
    public interface IAsyncCommand : ICommandBase
    {
        bool IsExecuting { get; }

        Task<bool> CanExecuteAsync(object parameter);

        Task ExecuteAsync(object parameter);
    }
}