using System.Runtime.Serialization;

namespace Lucile.Data.Metadata
{
    [DataContract]
    [ProtoBuf.ProtoContract(AsReferenceDefault = true)]
    public class MetadataModelTransfer
    {
        [DataMember(Order = 1)]
        public MetadataModel Model { get; set; }
    }
}