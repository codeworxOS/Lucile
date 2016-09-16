using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeworx.Input
{
    public class DelegateCommand<T> : DelegateCommand
    {
        private Func<T, bool> canExecute;

        private Action<T> executed;

        public DelegateCommand(Action<T> executed, Func<T, bool> canExecute = null, ICanExecuteRequeryStrategy strategy = null)
            : base(strategy)
        {
            if (executed == null)
                throw new ArgumentNullException("executed");

            this.canExecute = canExecute;
            this.executed = executed;
        }

        public override bool CanExecute(object parameter)
        {
            var result = false;
            if (this.canExecute == null)
                result = true;
            else {
                if (parameter != null && !(parameter is T))
                    throw new ArgumentOutOfRangeException("parameter", string.Format("Expected parameter of type {0}, got {1}", typeof(T), parameter == null ? (string)null : parameter.GetType().ToString()));

                result = this.canExecute((T)parameter);
                this.IsEnabled = result;
            }

            return result;
        }

        public override void Execute(object parameter)
        {
            if (parameter != null && !(parameter is T))
                throw new ArgumentOutOfRangeException("parameter", string.Format("Expected parameter of type {0}, got {1}", typeof(T), parameter == null ? (string)null : parameter.GetType().ToString()));

            if (!this.CanExecute(parameter))
                throw new InvalidOperationException("This should not happen... Please don't call the Execute Method while CanExecute is false!");

            this.executed((T)parameter);
        }
    }
}
