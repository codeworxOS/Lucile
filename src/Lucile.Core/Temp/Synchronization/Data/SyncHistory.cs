using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Codeworx.Synchronization.Data
{
    [DataContract]
    public class SyncHistory
    {
        [Key]
        [DataMember(Order = 1)]
        public Guid RequestId { get; set; }

        [DataMember(Order = 2)]
        public virtual SyncPartnership SyncPartnership { get; set; }

        [DataMember(Order = 3)]
        public Guid SourceId { get; set; }

        [DataMember(Order = 4)]
        public Guid TargetId { get; set; }

        [DataMember(Order = 5)]
        public DateTime SyncStarted { get; set; }

        [DataMember(Order = 6)]
        public DateTime? SyncFinished { get; set; }

#if (!NET4)
        [MaxLength(20)]
#endif
        [DataMember(Order = 7)]
        public byte[] SourceSyncVersion { get; set; }

#if (!NET4)
        [MaxLength(20)]
#endif
        [DataMember(Order = 8)]
        public byte[] TargetSyncVersion { get; set; }

        [DataMember(Order = 9)]
        public byte[] SourceModelConfiguration { get; set; }

        [DataMember(Order = 10)]
        public byte[] TargetModelConfiguration { get; set; }

        [DataMember(Order = 11)]
        public SyncState State { get; set; }

        [StringLength(4000)]
        [DataMember(Order = 12)]
        public string SyncInformation { get; set; }
    }
}
