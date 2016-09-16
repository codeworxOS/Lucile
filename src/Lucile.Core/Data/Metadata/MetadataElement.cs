using System.Runtime.Serialization;

namespace Lucile.Data.Metadata
{
    /// <summary>
    /// Funktionalität für ein Metadata-Element
    /// </summary>
    [KnownType(typeof(EntityMetadata))]
    [KnownType(typeof(PropertyMetadata))]
    [DataContract(IsReference = true)]
    [ProtoBuf.ProtoInclude(150, typeof(EntityMetadata))]
    [ProtoBuf.ProtoInclude(151, typeof(PropertyMetadata))]
    public class MetadataElement
    {
        /// <summary>
        /// Liefert oder setzt den Namen
        /// </summary>
        [DataMember(Order = 1)]
        public string Name { get; set; }

        /// <summary>
        /// Liefert das Objekt als String
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (this.Name != null)
            {
                return this.Name;
            }
            else
            {
                return base.ToString();
            }
        }
    }
}