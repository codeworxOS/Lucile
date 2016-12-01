using System.Collections.Generic;

namespace Lucile.Data.Metadata.Builder.Navigation
{
    public class ManyToOneNavigationBuilder : ChildNavigationBuilder
    {
        private readonly ManyNavigationBuilder _manyNavigationBuilder;

        public ManyToOneNavigationBuilder(ManyNavigationBuilder manyNavigationBuilder, string propertyName = null)
            : base(manyNavigationBuilder, propertyName)
        {
            _manyNavigationBuilder = manyNavigationBuilder;
        }

        public ManyToOneNavigationBuilder HasForeignKey(params string[] propertyNames)
        {
            _manyNavigationBuilder.NavigationPropertyBuilder.ForeignKey = new List<string>(propertyNames);
            return this;
        }

        public ManyToOneNavigationBuilder Required(bool value = true)
        {
            _manyNavigationBuilder.NavigationPropertyBuilder.TargetMultiplicity = value ? NavigationPropertyMultiplicity.One : NavigationPropertyMultiplicity.ZeroOrOne;
            return this;
        }
    }
}