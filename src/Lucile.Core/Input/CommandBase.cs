using System;

namespace Lucile.Input
{
    public abstract class CommandBase : NotificationObject, ICommandBase
    {
        private bool _isEnabled;

        public CommandBase(ICanExecuteRequeryStrategy strategy = null)
        {
            this.CanExecuteRequeryStrategy = strategy;
            if (this.CanExecuteRequeryStrategy != null)
            {
                this.CanExecuteRequeryStrategy.RegisterCommand(this);
            }
        }

        public CommandBase()
        {
        }

        public event EventHandler CanExecuteChanged;

        public ICanExecuteRequeryStrategy CanExecuteRequeryStrategy { get; private set; }

        public bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }

            protected set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    RaisePropertyChanged();
                }
            }
        }

        public abstract bool CanExecute(object parameter);

        public void InvalidateExecutionState()
        {
            RaiseCanExecuteChanged();
        }

        protected virtual void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}