using System.Runtime.Serialization;

namespace Lucile.Data.Metadata
{
    /// <summary>
    /// Funktionalität Metadaten für ein Blob-Property
    /// </summary>
    [DataContract(IsReference = true)]
    public class BlobProperty : ScalarProperty
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="enity"></param>
        public BlobProperty(EntityMetadata enity)
            : base(enity)
        {
        }

        internal BlobProperty()
        {
        }

        /// <summary>
        /// Liefert oder setzt die Länge
        /// </summary>
        [DataMember(Order = 1)]
        public int? Length { get; set; }
    }
}