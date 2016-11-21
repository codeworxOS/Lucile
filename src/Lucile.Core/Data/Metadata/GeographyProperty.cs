using Lucile.Data.Metadata.Builder;

namespace Lucile.Data.Metadata
{
    public class GeographyProperty : ScalarProperty
    {
        internal GeographyProperty(EntityMetadata entity, GeographyPropertyBuilder builder, bool isPrimaryKey)
            : base(entity, builder, isPrimaryKey)
        {
        }
    }
}