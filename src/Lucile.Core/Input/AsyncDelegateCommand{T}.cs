using System;
using System.Threading.Tasks;

namespace Lucile.Input
{
    public class AsyncDelegateCommand<T> : AsyncCommand
    {
        private readonly Func<T, bool> _canExecute;
        private readonly Func<T, Task<bool>> _canExecuteAsync;
        private readonly Func<T, Task> _execute;

        public AsyncDelegateCommand(Func<T, Task> execute, Func<T, bool> canExecute = null, ICanExecuteRequeryStrategy strategy = null)
            : base(false, strategy)
        {
            this._execute = execute;
            this._canExecute = canExecute;
            this.IsEnabled = this._canExecute == null;
        }

        public AsyncDelegateCommand(Func<T, Task> execute, Func<T, Task<bool>> canExecute)
            : base(canExecute != null, null)
        {
            this._execute = execute;
            this._canExecuteAsync = canExecute;
        }

        public AsyncDelegateCommand(Func<T, Task> execute, Func<T, Task<bool>> canExecute, ICanExecuteRequeryStrategy strategy)
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

            if (parameter != null && !(parameter is T))
            {
                throw new ArgumentOutOfRangeException(nameof(parameter), string.Format("Expected parameter of type {0}, got {1}", typeof(T), parameter == null ? (string)null : parameter.GetType().ToString()));
            }

            return this._canExecute((T)parameter);
        }

        protected override async Task<bool> OnCanExecuteAsync(object parameter)
        {
            if (this._canExecuteAsync == null)
            {
                return true;
            }

            if (parameter != null && !(parameter is T))
            {
                throw new ArgumentOutOfRangeException(nameof(parameter), string.Format("Expected parameter of type {0}, got {1}", typeof(T), parameter == null ? (string)null : parameter.GetType().ToString()));
            }

            return await this._canExecuteAsync((T)parameter);
        }

        protected override async Task OnExecuteAsync(object parameter)
        {
            if (parameter != null && !(parameter is T))
            {
                throw new ArgumentOutOfRangeException(nameof(parameter), string.Format("Expected parameter of type {0}, got {1}", typeof(T), parameter == null ? (string)null : parameter.GetType().ToString()));
            }

            await this._execute((T)parameter);
        }
    }
}