using System.Runtime.Serialization;

namespace Lucile.Data.Metadata.Builder
{
    [DataContract(IsReference = true)]
    public class GuidPropertyBuilder : ScalarPropertyBuilder
    {
        protected override ScalarProperty MapToProperty(EntityMetadata entity, bool isPrimaryKey)
        {
            return new GuidProperty(entity, this, isPrimaryKey);
        }
    }
}