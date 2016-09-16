using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeworx.Input
{
    public class DelegateCommand : CommandBase, ICommand
    {
        private Func<bool> canExecute;

        private Action executed;

        protected DelegateCommand(ICanExecuteRequeryStrategy strategy) : base(strategy) { }

        protected DelegateCommand() { }

        public DelegateCommand(Action executed, Func<bool> canExecute = null)
            : this(executed, canExecute, null)
        {
        }

        public DelegateCommand(Action executed, Func<bool> canExecute, ICanExecuteRequeryStrategy strategy = null)
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
            else
                result = this.canExecute();

            this.IsEnabled = result;
            return result;
        }

        public virtual void Execute(object parameter)
        {
            if (!this.CanExecute(parameter))
                throw new InvalidOperationException("This should not happen... Please don't call the Execute Method while CanExecute is false!");

            this.executed();
        }
    }
}
