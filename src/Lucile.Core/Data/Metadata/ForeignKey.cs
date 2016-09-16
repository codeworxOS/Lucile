using System.Runtime.Serialization;

namespace Lucile.Data.Metadata
{
    /// <summary>
    /// Funktionalität Metadaten für einen Foreign-Key
    /// </summary>
    [DataContract(IsReference = true)]
    [ProtoBuf.ProtoContract(AsReferenceDefault = true)]
    public class ForeignKey
    {
        /// <summary>
        /// Liefert oder setzt das abhängige Skalar-Property
        /// </summary>
        [DataMember(Order = 2)]
        public ScalarProperty Dependant { get; set; }

        /// <summary>
        /// Liefert oder setzt den Principal
        /// </summary>
        [DataMember(Order = 1)]
        public ScalarProperty Principal { get; set; }
    }
}