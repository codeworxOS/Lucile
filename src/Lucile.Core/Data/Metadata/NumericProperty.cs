using System.Runtime.Serialization;

namespace Lucile.Data.Metadata
{
    /// <summary>
    /// Funktionalität Metadaten für ein numersiches Property
    /// </summary>
    [DataContract(IsReference = true)]
    public class NumericProperty : ScalarProperty
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="enity"></param>
        public NumericProperty(EntityMetadata enity)
            : base(enity)
        {
        }

        internal NumericProperty()
        {
        }

        [DataMember(Order = 3)]
        public NumericPropertyType NumericPropertyType { get; set; }

        /// <summary>
        /// Liefert oder setzt die Präzision
        /// </summary>
        [DataMember(Order = 1)]
        public byte? Precision { get; set; }

        /// <summary>
        /// Liefert oder setzt die Länge
        /// </summary>
        [DataMember(Order = 2)]
        public byte? Scale { get; set; }
    }
}