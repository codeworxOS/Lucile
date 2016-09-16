using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Codeworx.Core;

namespace Codeworx.Input
{
    public abstract class AsyncCommand : CommandBase, IAsyncCommand
    {
        private bool isExecuting;

        private object executingLocker = new object();

        private bool asyncCanExecute;

        public AsyncCommand()
        {
            this.IsEnabled = false;
            this.asyncCanExecute = true;
        }

        public AsyncCommand(bool asyncCanExecute, ICanExecuteRequeryStrategy strategy)
            : base(strategy)
        {
            this.IsEnabled = false;
            this.asyncCanExecute = asyncCanExecute;
        }

        public async Task ExecuteAsync(object parameter)
        {
            lock (executingLocker) {
                if (IsExecuting || !IsEnabled) {
                    return;
                } else {
                    IsExecuting = true;
                    IsEnabled = false;
                }
            }
            bool canExecute = true;

            try {
                canExecute = await OnCanExecuteAsync(parameter);

                if (canExecute) {
                    await OnExecuteAsync(parameter);
                }

            } finally {
                lock (executingLocker) {
                    IsExecuting = false;
                    IsEnabled = canExecute;
                }
            }
        }

        public async Task<bool> CanExecuteAsync(object parameter)
        {
            lock (executingLocker) {
                if (IsExecuting)
                    return false;
            }

            bool canExecute = false;

            if (asyncCanExecute) {
                canExecute = await OnCanExecuteAsync(parameter);
            } else {
                canExecute = OnCanExecute(parameter);
            }
            lock (executingLocker) {
                if (!IsExecuting) {
                    IsEnabled = canExecute;
                }
            }
            return canExecute;
        }

        protected abstract Task OnExecuteAsync(object parameter);

        protected abstract Task<bool> OnCanExecuteAsync(object parameter);

        protected virtual bool OnCanExecute(object parameter)
        {
            if (!this.asyncCanExecute) {
                throw new NotImplementedException();
            }
            return true;
        }

        protected override void OnPropertyChanged(string propertyName = null)
        {
            switch (propertyName) {
                case "IsEnabled":
                    RaiseCanExecuteChanged();
                    break;
            }

            base.OnPropertyChanged(propertyName);
        }

        public bool IsExecuting
        {
            get { return isExecuting; }
            private set
            {
                if (isExecuting != value) {
                    isExecuting = value;
#if(!NET4 && !SILVERLIGHT)
                    OnPropertyChanged();
#else
                    OnPropertyChanged("IsExecuting");
#endif
                }
            }
        }

        public override bool CanExecute(object parameter)
        {
            lock (executingLocker) {
                if (IsExecuting)
                    return false;
            }

            if (asyncCanExecute) {
                CanExecuteAsync(parameter).InvokeAsync();
            } else {
                var canExecute = OnCanExecute(parameter);
                lock (executingLocker) {
                    if (!IsExecuting) {
                        this.IsEnabled = canExecute;
                    }
                }
            }
            return this.IsEnabled;
        }
    }
}
