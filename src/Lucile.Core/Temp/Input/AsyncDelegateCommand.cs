using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeworx.Input
{
    public class AsyncDelegateCommand : AsyncCommand
    {
        private Func<Task<bool>> canExecuteAsync;

        private Func<bool> canExecute;

        private Func<Task> execute;

        private Func<object, Task<bool>> oldCanExecute;

        private Func<object, Task> oldExecute;

        protected AsyncDelegateCommand()
        {

        }

        public AsyncDelegateCommand(Func<Task> execute, Func<bool> canExecute = null, ICanExecuteRequeryStrategy strategy = null)
            : base(false, strategy)
        {
            this.execute = execute;
            this.canExecute = canExecute;
            this.IsEnabled = this.canExecute == null;
        }

        public AsyncDelegateCommand(Func<Task> execute, Func<Task<bool>> canExecute) :
            this(execute, canExecute, null)
        {

        }

        public AsyncDelegateCommand(Func<Task> execute, Func<Task<bool>> canExecute, ICanExecuteRequeryStrategy strategy)
            : base(true, strategy)
        {
            if (canExecute == null)
                throw new ArgumentNullException("canExecute");

            this.execute = execute;
            this.canExecuteAsync = canExecute;
        }

        [Obsolete("If you have a command parameter of type object use AsyncDelegateCommand<object> instead. This constructor will be removed in future Versions of this assembly.", false)]
        public AsyncDelegateCommand(Func<object, Task> execute, Func<object, Task<bool>> canExecute = null)
        {
            this.oldExecute = execute;
            this.oldCanExecute = canExecute;
        }

        protected override async Task OnExecuteAsync(object parameter)
        {
            if (this.oldExecute != null)
                await oldExecute(parameter);
            else
                await execute();
        }

        protected override async Task<bool> OnCanExecuteAsync(object parameter)
        {
            if (canExecuteAsync == null && oldCanExecute == null)
                return true;

            if (oldCanExecute != null)
                return await oldCanExecute(parameter);
            else
                return await canExecuteAsync();
        }

        protected override bool OnCanExecute(object parameter)
        {
            if (this.canExecute == null)
                return true;

            return this.canExecute();
        }
    }
}
