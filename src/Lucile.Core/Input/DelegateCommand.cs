using System;

namespace Lucile.Input
{
    public class DelegateCommand : CommandBase, ICommand
    {
        private readonly Func<bool> _canExecute;

        private readonly Action _executed;

        public DelegateCommand(Action executed, Func<bool> canExecute = null)
            : this(executed, canExecute, null)
        {
        }

        public DelegateCommand(Action executed, Func<bool> canExecute, ICanExecuteRequeryStrategy strategy = null)
            : base(strategy)
        {
            if (executed == null)
            {
                throw new ArgumentNullException(nameof(executed));
            }

            this._canExecute = canExecute;
            this._executed = executed;
        }

        protected DelegateCommand(ICanExecuteRequeryStrategy strategy)
            : base(strategy)
        {
        }

        protected DelegateCommand()
        {
        }

        public override bool CanExecute(object parameter)
        {
            var result = false;
            if (this._canExecute == null)
            {
                result = true;
            }
            else
            {
                result = this._canExecute();
            }

            this.IsEnabled = result;
            return result;
        }

        public virtual void Execute(object parameter)
        {
            if (!this.CanExecute(parameter))
            {
                throw new InvalidOperationException("This should not happen... Please don't call the Execute Method while CanExecute is false!");
            }

            this._executed();
        }
    }
}