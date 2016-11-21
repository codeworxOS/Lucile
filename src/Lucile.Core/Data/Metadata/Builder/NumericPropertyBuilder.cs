using System.Runtime.Serialization;

namespace Lucile.Data.Metadata.Builder
{
    [DataContract(IsReference = true)]
    public class NumericPropertyBuilder : ScalarPropertyBuilder
    {
        [DataMember(Order = 1)]
        public NumericPropertyType NumericType { get; set; }

        [DataMember(Order = 2)]
        public byte Precision { get; set; }

        [DataMember(Order = 3)]
        public byte Scale { get; set; }

        protected override ScalarProperty MapToProperty(EntityMetadata entity, bool isPrimaryKey)
        {
            return new NumericProperty(entity, this, isPrimaryKey);
        }
    }
}