using System.Runtime.Serialization;

namespace Lucile.Data.Metadata
{
    [DataContract(IsReference = true)]
    public class GuidProperty : ScalarProperty
    {
        public GuidProperty(EntityMetadata entity)
            : base(entity)
        {
        }

        internal GuidProperty()
        {
        }
    }
}