using System.Collections.Generic;

namespace Lucile.Data.Metadata.Builder.Navigation
{
    public class OneToManyNavigationBuilder : ChildNavigationBuilder
    {
        private readonly OneNavigationBuilder _oneBuilder;

        public OneToManyNavigationBuilder(OneNavigationBuilder oneBuilder, string propertyName = null)
            : base(oneBuilder, propertyName)
        {
            _oneBuilder = oneBuilder;
            _oneBuilder.NavigationPropertyBuilder.TargetMultiplicity = NavigationPropertyMultiplicity.Many;
        }

        public void HasForeignKey(params string[] propertyNames)
        {
            _oneBuilder.NavigationPropertyBuilder.ForeignKey = new List<string>(propertyNames);
        }

        public OneToManyNavigationBuilder Optional(bool value = true)
        {
            _oneBuilder.Optional(value);
            return this;
        }
    }
}