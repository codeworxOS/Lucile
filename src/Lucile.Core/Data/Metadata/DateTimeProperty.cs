using Lucile.Data.Metadata.Builder;

namespace Lucile.Data.Metadata
{
    public class DateTimeProperty : ScalarProperty
    {
        internal DateTimeProperty(EntityMetadata entity, DateTimePropertyBuilder builder, bool isPrimaryKey)
            : base(entity, builder, isPrimaryKey)
        {
            DateTimePropertyType = builder.DateTimeType;
        }

        public DateTimePropertyType DateTimePropertyType { get; }
    }
}