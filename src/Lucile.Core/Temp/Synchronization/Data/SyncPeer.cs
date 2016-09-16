using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Codeworx.Synchronization.Data
{
    [DataContract(IsReference = true)]
    [ProtoBuf.ProtoContract(AsReferenceDefault = true)]
    public abstract class SyncPeer
    {
        public SyncPeer()
        {

        }

        [DataMember(Order = 1)]
        public Guid Id { get; set; }

        [DataMember(Order = 2)]
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
    }

    [DataContract(IsReference = true)]
    public class SelfSyncPeer : SyncPeer
    {

    }

    [DataContract(IsReference = true)]
    public class RemoteSyncPeer : SyncPeer
    {
    }
}
