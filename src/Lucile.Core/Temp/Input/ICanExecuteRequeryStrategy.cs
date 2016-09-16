using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeworx.Input
{
    public interface ICanExecuteRequeryStrategy
    {
        void RegisterCommand(ICommandBase command);
        void UnregisterCommand(ICommandBase command);
    }
}
