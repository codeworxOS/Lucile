using Lucile.Data.Metadata.Builder;

namespace Lucile.Data.Metadata
{
    public class TimeSpanProperty : ScalarProperty
    {
        internal TimeSpanProperty(EntityMetadata entity, TimeSpanPropertyBuilder builder, bool isPrimaryKey)
            : base(entity, builder, isPrimaryKey)
        {
        }
    }
}