using System.Runtime.Serialization;

namespace Lucile.Data.Metadata
{
    /// <summary>
    /// Funktionalität Metadaten für TextProperty
    /// </summary>
    [DataContract]
    public class TextProperty : ScalarProperty
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="enity"></param>
        public TextProperty(EntityMetadata enity)
            : base(enity)
        {
        }

        internal TextProperty()
        {
        }

        /// <summary>
        /// Liefert oder setzt die String-Länge
        /// </summary>
        [DataMember(Order = 1)]
        public int? StringLength { get; set; }
    }
}