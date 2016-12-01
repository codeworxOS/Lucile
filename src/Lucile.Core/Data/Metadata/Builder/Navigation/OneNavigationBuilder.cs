using System;
using System.Collections.Generic;

namespace Lucile.Data.Metadata.Builder.Navigation
{
    public class OneNavigationBuilder : RootNavigationBuilder
    {
        public OneNavigationBuilder(EntityMetadataBuilder entityBuilder, Type targetType, string propertyName)
            : base(entityBuilder, targetType, propertyName)
        {
        }

        public void HasForeignKey(string[] propertyNames)
        {
            NavigationPropertyBuilder.ForeignKey = new List<string>(propertyNames);
        }

        public OneNavigationBuilder Required(bool value = true)
        {
            NavigationPropertyBuilder.Multiplicity = value ? NavigationPropertyMultiplicity.One : NavigationPropertyMultiplicity.ZeroOrOne;
            NavigationPropertyBuilder.Nullable = value;
            return this;
        }

        public OneToOneNavigationBuilder WithDependant(string propertyName = null)
        {
            return new OneToOneNavigationBuilder(this, false, propertyName);
        }

        public OneToManyNavigationBuilder WithMany(string propertyName = null)
        {
            return new OneToManyNavigationBuilder(this, propertyName);
        }

        public OneToOneNavigationBuilder WithPrincipal(string propertyName = null)
        {
            return new OneToOneNavigationBuilder(this, true, propertyName);
        }
    }
}