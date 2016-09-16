using System;
using System.Runtime.Serialization;

namespace Codeworx.Service
{
    [DataContract]
    public class CallAggregationResult
    {
        [DataMember]
        public Guid CallId { get; set; }

        [DataMember]
        public object Value { get; set; }

        [DataMember]
        public TimeSpan? Duration { get; set; }
    }
}
