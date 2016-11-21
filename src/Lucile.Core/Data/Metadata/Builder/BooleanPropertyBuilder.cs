using System.Runtime.Serialization;

namespace Lucile.Data.Metadata.Builder
{
    [DataContract(IsReference = true)]
    public class BooleanPropertyBuilder : ScalarPropertyBuilder
    {
        protected override ScalarProperty MapToProperty(EntityMetadata entity, bool isPrimaryKey)
        {
            return new BooleanProperty(entity, this, isPrimaryKey);
        }
    }
}