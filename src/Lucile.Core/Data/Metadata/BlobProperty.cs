using Lucile.Data.Metadata.Builder;

namespace Lucile.Data.Metadata
{
    public class BlobProperty : ScalarProperty
    {
        internal BlobProperty(EntityMetadata enity, BlobPropertyBuilder builder, bool isPrimaryKey)
            : base(enity, builder, isPrimaryKey)
        {
            Length = builder.MaxLength;
        }

        public int? Length { get; }
    }
}