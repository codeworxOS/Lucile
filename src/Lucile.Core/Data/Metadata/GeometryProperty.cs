using System.Runtime.Serialization;

namespace Lucile.Data.Metadata
{
    [DataContract(IsReference = true)]
    public class GeometryProperty : ScalarProperty
    {
        public GeometryProperty(EntityMetadata entity)
            : base(entity)
        {
        }

        internal GeometryProperty()
        {
        }
    }
}