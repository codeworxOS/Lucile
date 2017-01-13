using System;
using System.ComponentModel;

namespace Lucile.Input
{
    public interface ICommandBase : INotifyPropertyChanged
    {
        event EventHandler CanExecuteChanged;

        ICanExecuteRequeryStrategy CanExecuteRequeryStrategy { get; }

        bool IsEnabled { get; }

        bool CanExecute(object parameter);

        void InvalidateExecutionState();
    }
}