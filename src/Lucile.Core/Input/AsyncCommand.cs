using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Lucile.Input
{
    public abstract class AsyncCommand : CommandBase, IAsyncCommand
    {
        private readonly bool _asyncCanExecute;
        private readonly object _executingLocker = new object();
        private bool _isExecuting;

        public AsyncCommand()
        {
            this.IsEnabled = false;
            this._asyncCanExecute = true;
        }

        public AsyncCommand(bool asyncCanExecute, ICanExecuteRequeryStrategy strategy)
            : base(strategy)
        {
            this.IsEnabled = false;
            this._asyncCanExecute = asyncCanExecute;
        }

        public bool IsExecuting
        {
            get
            {
                return _isExecuting;
            }

            private set
            {
                if (_isExecuting != value)
                {
                    _isExecuting = value;
                    RaisePropertyChanged();
                }
            }
        }

        public override bool CanExecute(object parameter)
        {
            lock (_executingLocker)
            {
                if (IsExecuting)
                {
                    return false;
                }
            }

            if (_asyncCanExecute)
            {
                CanExecuteAsync(parameter).Invoke();
            }
            else
            {
                var canExecute = OnCanExecute(parameter);
                lock (_executingLocker)
                {
                    if (!IsExecuting)
                    {
                        this.IsEnabled = canExecute;
                    }
                }
            }

            return this.IsEnabled;
        }

        public async Task<bool> CanExecuteAsync(object parameter)
        {
            lock (_executingLocker)
            {
                if (IsExecuting)
                {
                    return false;
                }
            }

            bool canExecute = false;

            if (_asyncCanExecute)
            {
                canExecute = await OnCanExecuteAsync(parameter);
            }
            else
            {
                canExecute = OnCanExecute(parameter);
            }

            lock (_executingLocker)
            {
                if (!IsExecuting)
                {
                    IsEnabled = canExecute;
                }
            }

            return canExecute;
        }

        public async Task ExecuteAsync(object parameter)
        {
            lock (_executingLocker)
            {
                if (IsExecuting || !IsEnabled)
                {
                    return;
                }
                else
                {
                    IsExecuting = true;
                    IsEnabled = false;
                }
            }

            bool canExecute = true;

            try
            {
                canExecute = await OnCanExecuteAsync(parameter);

                if (canExecute)
                {
                    await OnExecuteAsync(parameter);
                }
            }
            finally
            {
                lock (_executingLocker)
                {
                    IsExecuting = false;
                    IsEnabled = canExecute;
                }
            }
        }

        protected virtual bool OnCanExecute(object parameter)
        {
            if (!this._asyncCanExecute)
            {
                throw new NotImplementedException();
            }

            return true;
        }

        protected abstract Task<bool> OnCanExecuteAsync(object parameter);

        protected abstract Task OnExecuteAsync(object parameter);

        protected override void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            switch (propertyName)
            {
                case "IsEnabled":
                    RaiseCanExecuteChanged();
                    break;
            }

            base.RaisePropertyChanged(propertyName);
        }
    }
}