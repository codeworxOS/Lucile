using System.Collections.Generic;
using System.Linq;
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

        public void CopyFrom(NavigationPropertyMetadata source)
        {
            this.Target = new ClrTypeInfo(source.TargetEntity.ClrType);
            this.ForeignKey.Clear();
            this.ForeignKey.AddRange(source.ForeignKeyProperties.Select(p => p.Dependant.Name));
            this.Multiplicity = source.Multiplicity;
            this.Nullable = source.Nullable;
            this.TargetMultiplicity = source.TargetMultiplicity;
            this.TargetProperty = source.TargetNavigationProperty?.Name;
        }

        public void CopyFrom(NavigationPropertyBuilder source)
        {
            this.Target = source.Target;
            this.ForeignKey.Clear();
            this.ForeignKey.AddRange(source.ForeignKey);
            this.IsExcluded = source.IsExcluded;
            this.Multiplicity = source.Multiplicity;
            this.Nullable = source.Nullable;
            this.TargetMultiplicity = source.TargetMultiplicity;
            this.TargetProperty = source.TargetProperty;
        }

        internal NavigationPropertyMetadata ToNavigation(ModelCreationScope scope, EntityMetadata entityMetadata)
        {
            return new NavigationPropertyMetadata(scope, entityMetadata, this);
        }
    }
}