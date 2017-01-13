using System;

namespace Lucile.Input
{
    public class DelegateCommand<T> : DelegateCommand
    {
        private readonly Func<T, bool> _canExecute;

        private readonly Action<T> _executed;

        public DelegateCommand(Action<T> executed, Func<T, bool> canExecute = null, ICanExecuteRequeryStrategy strategy = null)
            : base(strategy)
        {
            if (executed == null)
            {
                throw new ArgumentNullException(nameof(executed));
            }

            this._canExecute = canExecute;
            this._executed = executed;
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
                if (parameter != null && !(parameter is T))
                {
                    throw new ArgumentOutOfRangeException(nameof(parameter), string.Format("Expected parameter of type {0}, got {1}", typeof(T), parameter == null ? (string)null : parameter.GetType().ToString()));
                }

                result = this._canExecute((T)parameter);
                this.IsEnabled = result;
            }

            return result;
        }

        public override void Execute(object parameter)
        {
            if (parameter != null && !(parameter is T))
            {
                throw new ArgumentOutOfRangeException(nameof(parameter), string.Format("Expected parameter of type {0}, got {1}", typeof(T), parameter == null ? (string)null : parameter.GetType().ToString()));
            }

            if (!this.CanExecute(parameter))
            {
                throw new InvalidOperationException("This should not happen... Please don't call the Execute Method while CanExecute is false!");
            }

            this._executed((T)parameter);
        }
    }
}