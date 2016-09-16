using System.Runtime.Serialization;

namespace Lucile.Data.Metadata
{
    [DataContract(IsReference = true)]
    public class GeographyProperty : ScalarProperty
    {
        public GeographyProperty(EntityMetadata entity)
            : base(entity)
        {
        }

        internal GeographyProperty()
        {
        }
    }
}