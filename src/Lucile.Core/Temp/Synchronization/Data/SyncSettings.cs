using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Codeworx.Synchronization.Data
{
    [DataContract]
    public class SyncSettings
    {
        [Key]
        [DataMember(Order = 1)]
        public Guid Id { get; set; }

        [StringLength(255)]
        [DataMember(Order = 2)]
        public string Name { get; set; }
    }
}
