using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Codeworx.Synchronization.Data
{
    [DataContract(IsReference = true)]
    [ProtoBuf.ProtoContract(AsReferenceDefault = true)]
    public class SyncParameter
    {
        [DataMember(Order = 1)]
        [Key]
#if(!NET4)
        [System.ComponentModel.DataAnnotations.Schema.Column(Order = 0)]
#endif
        public Guid SourceId { get; set; }

        [DataMember(Order = 2)]
        [Key]
#if (!NET4)
        [System.ComponentModel.DataAnnotations.Schema.Column(Order = 1)]
#endif
        public Guid TargetId { get; set; }

        [DataMember(Order = 3)]
        [Key]
#if (!NET4)
        [System.ComponentModel.DataAnnotations.Schema.Column(Order = 2)]
#endif
        public Guid ParameterId { get; set; }

        [DataMember(Order = 4)]
        public SyncPartnership SyncPartnership { get; set; }

        [DataMember(Order = 5)]
        public byte[] Values { get; set; }
    }
}
