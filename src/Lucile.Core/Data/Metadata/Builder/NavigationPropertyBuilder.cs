using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Lucile.Data.Metadata.Builder
{
    [DataContract(IsReference = true)]
    public class NavigationPropertyBuilder : PropertyMetadataBuilder
    {
        public NavigationPropertyBuilder()
        {
            this.ForeignKey = new List<string>();
        }

        [DataMember(Order = 5)]
        public List<string> ForeignKey { get; set; }

        [DataMember(Order = 2)]
        public NavigationPropertyMultiplicity Multiplicity { get; set; }

        [DataMember(Order = 1)]
        public ClrTypeInfo Target { get; set; }

        [DataMember(Order = 3)]
        public NavigationPropertyMultiplicity TargetMultiplicity { get; set; }

        [DataMember(Order = 4)]
        public string TargetProperty { get; set; }

        internal NavigationPropertyMetadata ToNavigation(ModelCreationScope scope, EntityMetadata entityMetadata)
        {
            return new NavigationPropertyMetadata(scope, entityMetadata, this);
        }
    }
}