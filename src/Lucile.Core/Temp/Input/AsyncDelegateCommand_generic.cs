using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeworx.Input
{
    public class AsyncDelegateCommand<T> : AsyncCommand
    {
        private Func<T, Task<bool>> canExecuteAsync;

        private Func<T, bool> canExecute;

        private Func<T, Task> execute;

        protected AsyncDelegateCommand()
        {

        }

        public AsyncDelegateCommand(Func<T, Task> execute, Func<T, bool> canExecute = null, ICanExecuteRequeryStrategy strategy = null)
            : base(false, strategy)
        {
            this.execute = execute;
            this.canExecute = canExecute;
            this.IsEnabled = this.canExecute == null;
        }

        public AsyncDelegateCommand(Func<T, Task> execute, Func<T, Task<bool>> canExecute)
            : base(canExecute != null, null)
        {
            this.execute = execute;
            this.canExecuteAsync = canExecute;
        }

        public AsyncDelegateCommand(Func<T, Task> execute, Func<T, Task<bool>> canExecute, ICanExecuteRequeryStrategy strategy)
            : base(true, strategy)
        {
            if (canExecute == null)
                throw new ArgumentNullException("canExecute");

            this.execute = execute;
            this.canExecuteAsync = canExecute;
        }

        protected override async Task OnExecuteAsync(object parameter)
        {
            if (parameter != null && !(parameter is T))
                throw new ArgumentOutOfRangeException("parameter", string.Format("Expected parameter of type {0}, got {1}", typeof(T), parameter == null ? (string)null : parameter.GetType().ToString()));

            await this.execute((T)parameter);
        }

        protected override async Task<bool> OnCanExecuteAsync(object parameter)
        {
            if (this.canExecuteAsync == null)
                return true;

            if (parameter != null && !(parameter is T))
                throw new ArgumentOutOfRangeException("parameter", string.Format("Expected parameter of type {0}, got {1}", typeof(T), parameter == null ? (string)null : parameter.GetType().ToString()));

            return await this.canExecuteAsync((T)parameter);
        }

        protected override bool OnCanExecute(object parameter)
        {
            if (this.canExecute == null)
                return true;

            if (parameter != null && !(parameter is T))
                throw new ArgumentOutOfRangeException("parameter", string.Format("Expected parameter of type {0}, got {1}", typeof(T), parameter == null ? (string)null : parameter.GetType().ToString()));

            return this.canExecute((T)parameter);
        }
    }
}
