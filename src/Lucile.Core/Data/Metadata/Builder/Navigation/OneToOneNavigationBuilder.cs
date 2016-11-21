namespace Lucile.Data.Metadata.Builder.Navigation
{
    public class OneToOneNavigationBuilder : ChildNavigationBuilder
    {
        private readonly OneNavigationBuilder _oneBuilder;

        public OneToOneNavigationBuilder(OneNavigationBuilder oneBuilder, bool isPrincipalEnd, string propertyName = null)
            : base(oneBuilder, propertyName)
        {
            _oneBuilder.NavigationPropertyBuilder.TargetMultiplicity = isPrincipalEnd ? NavigationPropertyMultiplicity.One : NavigationPropertyMultiplicity.ZeroOrOne;
            _oneBuilder = oneBuilder;
        }
    }
}