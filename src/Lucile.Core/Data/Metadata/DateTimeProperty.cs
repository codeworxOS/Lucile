using System.Runtime.Serialization;

namespace Lucile.Data.Metadata
{
    [DataContract(IsReference = true)]
    public class DateTimeProperty : ScalarProperty
    {
        public DateTimeProperty(EntityMetadata entity)
            : base(entity)
        {
        }

        internal DateTimeProperty()
        {
        }

        [DataMember(Order = 1)]
        public DateTimePropertyType DateTimePropertyType { get; set; }
    }
}