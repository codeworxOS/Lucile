using System.Collections.Generic;

namespace Lucile.Data.Metadata.Builder.Navigation
{
    public class ManyToOneNavigationBuilder : ChildNavigationBuilder
    {
        private readonly ManyNavigationBuilder _manyNavigationBuilder;
        private readonly NavigationPropertyBuilder _navigationPropertyBuilder;

        public ManyToOneNavigationBuilder(ManyNavigationBuilder manyNavigationBuilder, string propertyName = null)
            : base(manyNavigationBuilder, propertyName)
        {
            _manyNavigationBuilder = manyNavigationBuilder;
            if (propertyName != null)
            {
                _navigationPropertyBuilder = _manyNavigationBuilder.EntityBuilder.ModelBuilder.Entity(_manyNavigationBuilder.TargetType).Navigation(propertyName);
                _navigationPropertyBuilder.TargetMultiplicity = NavigationPropertyMultiplicity.Many;
                _navigationPropertyBuilder.Multiplicity = NavigationPropertyMultiplicity.One;
            }
        }

        public ManyToOneNavigationBuilder HasForeignKey(params string[] propertyNames)
        {
            _navigationPropertyBuilder.ForeignKey = new List<string>(propertyNames);
            return this;
        }

        public ManyToOneNavigationBuilder Required(bool value = true)
        {
            _manyNavigationBuilder.NavigationPropertyBuilder.TargetMultiplicity = value ? NavigationPropertyMultiplicity.One : NavigationPropertyMultiplicity.ZeroOrOne;
            _navigationPropertyBuilder.Multiplicity = value ? NavigationPropertyMultiplicity.One : NavigationPropertyMultiplicity.ZeroOrOne;
            return this;
        }
    }
}