namespace Lucile.ViewModel
{
    public class DeactivateArgs
    {
        public DeactivateArgs(object newContent)
        {
            NewContent = newContent;
        }

        public bool Cancel { get; set; }

        public object NewContent { get; private set; }
    }
}