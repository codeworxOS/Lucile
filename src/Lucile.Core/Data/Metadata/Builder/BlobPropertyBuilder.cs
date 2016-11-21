using System.Runtime.Serialization;

namespace Lucile.Data.Metadata.Builder
{
    [DataContract(IsReference = true)]
    public class BlobPropertyBuilder : ScalarPropertyBuilder
    {
        public BlobPropertyBuilder()
        {
            this.Nullable = true;
        }

        [DataMember(Order = 1)]
        public int? MaxLength { get; set; }

        protected override ScalarProperty MapToProperty(EntityMetadata entity, bool isPrimaryKey)
        {
            return new BlobProperty(entity, this, isPrimaryKey);
        }
    }
}