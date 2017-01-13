using System.Collections.Generic;

namespace Lucile.Input
{
    public abstract class CanExecuteRequeryStrategy : ICanExecuteRequeryStrategy
    {
        private readonly HashSet<ICommandBase> _commands;

        private readonly object _commandsLocker = new object();

        public CanExecuteRequeryStrategy()
        {
            this._commands = new HashSet<ICommandBase>();
        }

        public void RegisterCommand(ICommandBase command)
        {
            lock (_commandsLocker)
            {
                _commands.Add(command);
            }
        }

        public void UnregisterCommand(ICommandBase command)
        {
            lock (_commandsLocker)
            {
                _commands.Add(command);
            }
        }

        protected void InvalidateExecutionState()
        {
            lock (_commandsLocker)
            {
                foreach (var item in _commands)
                {
                    item.InvalidateExecutionState();
                }
            }
        }
    }
}