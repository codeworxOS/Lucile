using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Codeworx.Service
{
    [DataContract]
    public class CallAggregationCallDescription
    {
        public CallAggregationCallDescription(string methodName, IEnumerable<object> parameters) : this()
        {
            this.MethodName = methodName;
            foreach (var item in parameters) {
                this.Parameters.Add(item);
            }
        }

        public CallAggregationCallDescription()
        {
            this.CallId = Guid.NewGuid();
            this.Parameters = new Collection<object>();
        }

        [DataMember]
        public Guid CallId { get; set; }

        [DataMember]
        public string MethodName { get; set; }

        [DataMember]
        public Collection<object> Parameters { get; private set; }

        [DataMember]
        public bool MeasureDuration { get; set; }

        public CallAggregationResult CreateResult(object value)
        {
            return new CallAggregationResult { CallId = CallId, Value = value };
        }
    }
}
