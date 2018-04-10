namespace Lucile.Input
{
    public interface ICommand : ICommandBase
    {
        new bool CanExecute(object parameter);

        void Execute(object parameter);
    }
}