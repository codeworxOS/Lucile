using System.Runtime.Serialization;

namespace Lucile.Data.Metadata.Builder
{
    [DataContract(IsReference = true)]
    public class EnumPropertyBuilder : ScalarPropertyBuilder
    {
        [DataMember(Order = 2)]
        public ClrTypeInfo EnumTypeInfo { get; set; }

        [DataMember(Order = 1)]
        public NumericPropertyType UnderlyingNumericType { get; set; }

        protected override ScalarProperty MapToProperty(EntityMetadata entity, bool isPrimaryKey)
        {
            return new EnumProperty(entity, this, isPrimaryKey);
        }
    }
}