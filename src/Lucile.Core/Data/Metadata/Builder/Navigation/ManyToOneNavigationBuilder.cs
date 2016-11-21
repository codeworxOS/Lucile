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
            if (!manyNavigationBuilder.NavigationPropertyBuilder.TargetMultiplicity.HasValue)
            {
                manyNavigationBuilder.NavigationPropertyBuilder.TargetMultiplicity = NavigationPropertyMultiplicity.One;
            }
        }

        public ManyToOneNavigationBuilder HasForeignKey(params string[] propertyNames)
        {
            _manyNavigationBuilder.NavigationPropertyBuilder.ForeignKey = new List<string>(propertyNames);
            return this;
        }

        public ManyToOneNavigationBuilder Optional(bool value = true)
        {
            _manyNavigationBuilder.NavigationPropertyBuilder.TargetMultiplicity = value ? NavigationPropertyMultiplicity.ZeroOrOne : NavigationPropertyMultiplicity.One;
            return this;
        }
    }
}