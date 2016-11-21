namespace Lucile.Data.Metadata.Builder.Navigation
{
    public class ManyToManyNavigationBuilder : ChildNavigationBuilder
    {
        private readonly ManyNavigationBuilder _manyNavigationBuilder;

        public ManyToManyNavigationBuilder(ManyNavigationBuilder manyNavigationBuilder, string propertyName = null)
            : base(manyNavigationBuilder, propertyName)
        {
            this._manyNavigationBuilder = manyNavigationBuilder;
            _manyNavigationBuilder.NavigationPropertyBuilder.TargetMultiplicity = NavigationPropertyMultiplicity.Many;
        }
    }
}