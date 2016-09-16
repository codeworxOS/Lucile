using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;

namespace Codeworx.Input
{
    public abstract class CanExecuteRequeryStrategy : ICanExecuteRequeryStrategy
    {
        public CanExecuteRequeryStrategy()
        {
            this.commands = new HashSet<ICommandBase>();
        }

        private object commandsLocker = new object();

        private HashSet<ICommandBase> commands;

        protected void InvalidateExecutionState()
        {
            lock (commandsLocker) {
                foreach (var item in commands) {
                    item.InvalidateExecutionState();
                }
            }
        }

        public void RegisterCommand(ICommandBase command)
        {
            lock (commandsLocker) {
                commands.Add(command);
            }
        }

        public void UnregisterCommand(ICommandBase command)
        {
            lock (commandsLocker) {
                commands.Add(command);
            }
        }
    }
}
