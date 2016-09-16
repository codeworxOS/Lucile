using Codeworx.Data.Entity;
using System;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;

namespace Codeworx.Synchronization.Data
{
    [DataContract(IsReference = true)]
    [ProtoBuf.ProtoContract(AsReferenceDefault = true)]
    public class SyncPartnership
    {
        public SyncPartnership()
        {
            this.SyncHistory = new TrackableCollection<SyncHistory>();
            this.SyncParameters = new TrackableCollection<SyncParameter>();
        }

        [DataMember(Order = 1)]
        [Key]
#if (!NET4)
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
        public DateTime Created { get; set; }

        [DataMember(Order = 4)]
        public Guid? ScenarioId { get; set; }

        [DataMember(Order = 5)]
        public virtual TrackableCollection<SyncHistory> SyncHistory { get; set; }

        [DataMember(Order = 5)]
        public virtual TrackableCollection<SyncParameter> SyncParameters { get; set; }
    }
}
