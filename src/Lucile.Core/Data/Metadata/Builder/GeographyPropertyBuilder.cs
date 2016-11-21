using System.Runtime.Serialization;

namespace Lucile.Data.Metadata.Builder
{
    [DataContract(IsReference = true)]
    public class GeographyPropertyBuilder : ScalarPropertyBuilder
    {
        protected override ScalarProperty MapToProperty(EntityMetadata entity, bool isPrimaryKey)
        {
            return new GeographyProperty(entity, this, isPrimaryKey);
        }
    }
}