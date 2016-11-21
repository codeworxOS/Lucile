using System;

namespace Lucile.Data.Metadata.Builder.Navigation
{
    public class ManyNavigationBuilder : RootNavigationBuilder
    {
        public ManyNavigationBuilder(EntityMetadataBuilder entityBuilder, Type targetType, string propertyName)
            : base(entityBuilder, targetType, propertyName)
        {
            NavigationPropertyBuilder.Multiplicity = NavigationPropertyMultiplicity.Many;
            NavigationPropertyBuilder.Nullable = false;
        }

        public ManyToManyNavigationBuilder WithMany(string propertyName)
        {
            return new ManyToManyNavigationBuilder(this, propertyName);
        }

        public ManyToOneNavigationBuilder WithOne(string propertyName)
        {
            return new ManyToOneNavigationBuilder(this, propertyName);
        }
    }
}