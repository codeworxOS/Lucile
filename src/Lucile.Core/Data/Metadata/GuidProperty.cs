using Lucile.Data.Metadata.Builder;

namespace Lucile.Data.Metadata
{
    public class GuidProperty : ScalarProperty
    {
        public GuidProperty(EntityMetadata entity, GuidPropertyBuilder builder, bool isPrimaryKey)
            : base(entity, builder, isPrimaryKey)
        {
        }
    }
}