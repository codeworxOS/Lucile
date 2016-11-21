namespace Lucile.Data.Metadata.Builder.Navigation
{
    public class ChildNavigationBuilder : NavigationBuilder
    {
        private readonly RootNavigationBuilder _root;

        public ChildNavigationBuilder(RootNavigationBuilder root, string propertyName = null)
            : base(propertyName)
        {
            _root = root;
            root.NavigationPropertyBuilder.TargetProperty = propertyName;
        }
    }
}