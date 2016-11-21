using Lucile.Data.Metadata.Builder;

namespace Lucile.Data.Metadata
{
    public class GeometryProperty : ScalarProperty
    {
        internal GeometryProperty(EntityMetadata entity, GeometryPropertyBuilder builder, bool isPrimaryKey)
            : base(entity, builder, isPrimaryKey)
        {
        }
    }
}