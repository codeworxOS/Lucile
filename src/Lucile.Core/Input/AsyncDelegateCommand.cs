using System;
using System.Threading.Tasks;

namespace Lucile.Input
{
    public class AsyncDelegateCommand : AsyncCommand
    {
        private readonly Func<bool> _canExecute;
        private readonly Func<Task<bool>> _canExecuteAsync;
        private readonly Func<Task> _execute;

        public AsyncDelegateCommand(Func<Task> execute, Func<bool> canExecute = null, ICanExecuteRequeryStrategy strategy = null)
            : base(false, strategy)
        {
            this._execute = execute;
            this._canExecute = canExecute;
            this.IsEnabled = this._canExecute == null;
        }

        public AsyncDelegateCommand(Func<Task> execute, Func<Task<bool>> canExecute)
            : this(execute, canExecute, null)
        {
        }

        public AsyncDelegateCommand(Func<Task> execute, Func<Task<bool>> canExecute, ICanExecuteRequeryStrategy strategy)
            : base(true, strategy)
        {
            if (canExecute == null)
            {
                throw new ArgumentNullException(nameof(canExecute));
            }

            this._execute = execute;
            this._canExecuteAsync = canExecute;
        }

        protected AsyncDelegateCommand()
        {
        }

        protected override bool OnCanExecute(object parameter)
        {
            if (this._canExecute == null)
            {
                return true;
            }

            return this._canExecute();
        }

        protected override async Task<bool> OnCanExecuteAsync(object parameter)
        {
            if (_canExecuteAsync == null)
            {
                return true;
            }

            return await _canExecuteAsync();
        }

        protected override async Task OnExecuteAsync(object parameter)
        {
            await _execute();
        }
    }
}