using System.Runtime.Serialization;

namespace Lucile.Data.Metadata
{
    /// <summary>
    /// Funktionalität Metadata für ein Boolean-Property
    /// </summary>
    [DataContract(IsReference = true)]
    public class BooleanProperty : ScalarProperty
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="enity"></param>
        public BooleanProperty(EntityMetadata enity)
            : base(enity)
        {
        }

        internal BooleanProperty()
        {
        }
    }
}