using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeworx.Input
{
    public interface ICommand : ICommandBase
    {
        void Execute(object parameter);

        new bool CanExecute(object parameter);
    }
}
