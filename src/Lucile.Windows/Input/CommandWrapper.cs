using System;
using System.Threading.Tasks;
using Lucile.Input;
using Lucile.Threading;

namespace Sade.Windows.Input
{
    public class CommandWrapper : System.Windows.Input.ICommand
    {
        private readonly ICommandBase _baseCommand;
        private readonly IViewOperations _viewOperations;

        public CommandWrapper(ICommandBase command, IViewOperations viewOperations)
        {
            this._baseCommand = command;
            this._viewOperations = viewOperations;
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                this.CanExecuteInternal += value;
                this._baseCommand.CanExecuteChanged -= BaseCommand_CanExecuteChanged;
                this._baseCommand.CanExecuteChanged += BaseCommand_CanExecuteChanged;
            }

            remove
            {
                this.CanExecuteInternal -= value;
                this._baseCommand.CanExecuteChanged -= BaseCommand_CanExecuteChanged;
            }
        }

        private event EventHandler CanExecuteInternal;

        public bool CanExecute(object parameter)
        {
            if (this._baseCommand is Lucile.Input.ICommandBase command)
            {
                return command.CanExecute(parameter);
            }

            return true;
        }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void

        public async void Execute(object parameter)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            var asyncCommand = this._baseCommand as IAsyncCommand;
            if (this._baseCommand is Lucile.Input.ICommand command)
            {
                command.Execute(parameter);
            }
            else if (asyncCommand != null)
            {
                await asyncCommand.ExecuteAsync(parameter);
            }
        }

        private void BaseCommand_CanExecuteChanged(object sender, EventArgs e)
        {
            _viewOperations.ExecuteAsync(
                () =>
                {
                    this.CanExecuteInternal?.Invoke(this, EventArgs.Empty);
                }).Invoke();
        }
    }
}