using System.Runtime.Serialization;

namespace Lucile.Data.Metadata.Builder
{
    [DataContract(IsReference = true)]
    public class DateTimePropertyBuilder : ScalarPropertyBuilder
    {
        [DataMember(Order = 1)]
        public DateTimePropertyType DateTimeType { get; set; }

        protected override ScalarProperty MapToProperty(EntityMetadata entity, bool isPrimaryKey)
        {
            return new DateTimeProperty(entity, this, isPrimaryKey);
        }
    }
}