using System.Runtime.Serialization;

namespace Lucile.Data.Metadata.Builder
{
    [DataContract(IsReference = true)]
    public class GeometryPropertyBuilder : ScalarPropertyBuilder
    {
        protected override ScalarProperty MapToProperty(EntityMetadata entity, bool isPrimaryKey)
        {
            return new GeometryProperty(entity, this, isPrimaryKey);
        }
    }
}