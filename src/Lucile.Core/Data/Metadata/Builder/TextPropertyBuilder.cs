using System.Runtime.Serialization;

namespace Lucile.Data.Metadata.Builder
{
    [DataContract(IsReference = true)]
    public class TextPropertyBuilder : ScalarPropertyBuilder
    {
        public TextPropertyBuilder()
        {
            Nullable = true;
            Unicode = true;
        }

        [DataMember(Order = 2)]
        public bool FixedLength { get; set; }

        [DataMember(Order = 1)]
        public int? MaxLength { get; set; }

        [DataMember(Order = 3)]
        public bool Unicode { get; set; }

        protected override ScalarProperty MapToProperty(EntityMetadata entity, bool isPrimaryKey)
        {
            return new TextProperty(entity, this, isPrimaryKey);
        }
    }
}