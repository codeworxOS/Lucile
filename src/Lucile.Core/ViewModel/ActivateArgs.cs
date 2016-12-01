namespace Lucile.ViewModel
{
    public class ActivateArgs
    {
        public ActivateArgs(object previousContent)
        {
            this.PreviousContent = previousContent;
        }

        public object PreviousContent { get; private set; }
    }
}