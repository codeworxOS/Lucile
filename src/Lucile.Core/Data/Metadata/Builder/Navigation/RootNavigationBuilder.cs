using System;

namespace Lucile.Data.Metadata.Builder.Navigation
{
    public abstract class RootNavigationBuilder : NavigationBuilder
    {
        protected RootNavigationBuilder(EntityMetadataBuilder entityBuilder, Type targetType, string propertyName)
            : base(propertyName)
        {
            this.EntityBuilder = entityBuilder;
            this.TargetType = targetType;
            this.NavigationPropertyBuilder = entityBuilder.Navigation(propertyName);
        }

        public EntityMetadataBuilder EntityBuilder { get; }

        public NavigationPropertyBuilder NavigationPropertyBuilder { get; }

        public Type TargetType { get; }
    }
}