using System;

namespace Lucile.Data.Metadata.Builder.Navigation
{
    public class ManyNavigationBuilder : RootNavigationBuilder
    {
        public ManyNavigationBuilder(EntityMetadataBuilder entityBuilder, Type targetType, string propertyName)
            : base(entityBuilder, targetType, propertyName)
        {
            NavigationPropertyBuilder.Nullable = false;
        }

        public ManyToManyNavigationBuilder WithMany(string propertyName)
        {
            return new ManyToManyNavigationBuilder(this, propertyName);
        }

        public ManyToOneNavigationBuilder WithOne(string propertyName = null)
        {
            return new ManyToOneNavigationBuilder(this, propertyName);
        }
    }
}