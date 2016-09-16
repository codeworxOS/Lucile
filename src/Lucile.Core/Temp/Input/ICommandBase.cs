using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Codeworx.Input
{
    public interface ICommandBase : INotifyPropertyChanged
    {
        bool IsEnabled { get; }

        event EventHandler CanExecuteChanged;

        ICanExecuteRequeryStrategy CanExecuteRequeryStrategy { get; }

        bool CanExecute(object parameter);

        void InvalidateExecutionState();
    }
}
