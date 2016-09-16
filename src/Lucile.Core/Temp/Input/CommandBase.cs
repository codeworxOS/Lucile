using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Codeworx.Core;

namespace Codeworx.Input
{
    public abstract class CommandBase : NotificationObject, ICommandBase
    {
        public ICanExecuteRequeryStrategy CanExecuteRequeryStrategy { get; private set; }

        public CommandBase(ICanExecuteRequeryStrategy strategy = null)
        {
            this.CanExecuteRequeryStrategy = strategy;
            if (this.CanExecuteRequeryStrategy != null) {
                this.CanExecuteRequeryStrategy.RegisterCommand(this);
            }
        }

        public CommandBase()
        {

        }

        private bool isEnabled;

        public void InvalidateExecutionState()
        {
            RaiseCanExecuteChanged();
        }

        protected virtual void RaiseCanExecuteChanged()
        {
            if (this.CanExecuteChanged != null) {
                this.CanExecuteChanged(this, EventArgs.Empty);
            }
        }

        public abstract bool CanExecute(object parameter);

        public event EventHandler CanExecuteChanged;

        public bool IsEnabled
        {
            get { return isEnabled; }
            protected set
            {
                if (isEnabled != value) {
                    isEnabled = value;
#if(!NET4 && !SILVERLIGHT)
                    OnPropertyChanged();
#else
                    OnPropertyChanged("IsEnabled");
#endif
                }
            }
        }
    }
}
