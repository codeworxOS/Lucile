using System;
using System.Collections.Generic;

namespace Lucile.Data.Metadata.Builder.Navigation
{
    public class OneNavigationBuilder : RootNavigationBuilder
    {
        public OneNavigationBuilder(EntityMetadataBuilder entityBuilder, Type targetType, string propertyName)
            : base(entityBuilder, targetType, propertyName)
        {
            if (!NavigationPropertyBuilder.Multiplicity.HasValue)
            {
                NavigationPropertyBuilder.Multiplicity = NavigationPropertyMultiplicity.One;
            }
        }

        public void HasForeignKey(string[] propertyNames)
        {
            NavigationPropertyBuilder.ForeignKey = new List<string>(propertyNames);
        }

        public OneNavigationBuilder Optional(bool value = true)
        {
            NavigationPropertyBuilder.Multiplicity = value ? NavigationPropertyMultiplicity.ZeroOrOne : NavigationPropertyMultiplicity.One;
            NavigationPropertyBuilder.Nullable = value;
            return this;
        }

        public OneToOneNavigationBuilder WithDependant(string propertyName)
        {
            if (NavigationPropertyBuilder.Multiplicity == NavigationPropertyMultiplicity.ZeroOrOne)
            {
                throw new NotSupportedException("A principal end of a OneToOne relationship must not be optional.");
            }

            return new OneToOneNavigationBuilder(this, false, propertyName);
        }

        public OneToManyNavigationBuilder WithMany(string propertyName)
        {
            return new OneToManyNavigationBuilder(this, propertyName);
        }

        public OneToOneNavigationBuilder WithPrincipal(string propertyName)
        {
            return new OneToOneNavigationBuilder(this, true, propertyName);
        }
    }
}