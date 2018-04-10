namespace Lucile.Input
{
    public interface ICanExecuteRequeryStrategy
    {
        void RegisterCommand(ICommandBase command);

        void UnregisterCommand(ICommandBase command);
    }
}