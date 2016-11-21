using Lucile.Data.Metadata.Builder;

namespace Lucile.Data.Metadata
{
    public class BooleanProperty : ScalarProperty
    {
        internal BooleanProperty(EntityMetadata enity, BooleanPropertyBuilder builder, bool isPrimaryKey)
            : base(enity, builder, isPrimaryKey)
        {
        }
    }
}