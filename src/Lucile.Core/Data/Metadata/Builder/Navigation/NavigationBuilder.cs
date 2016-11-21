namespace Lucile.Data.Metadata.Builder.Navigation
{
    public abstract class NavigationBuilder
    {
        protected NavigationBuilder(string propertyName = null)
        {
            PropertyName = propertyName;
        }

        public string PropertyName { get; }
    }
}