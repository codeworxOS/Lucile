using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Lucile.Data.Metadata
{
    [DataContract]
    public class MultiValueKey : IEquatable<MultiValueKey>
    {
        public MultiValueKey()
        {
            this.Values = new Dictionary<string, object>();
        }

        [DataMember(Order = 1)]
        public Dictionary<string, object> Values { get; private set; }

        public override bool Equals(object obj)
        {
            var other = obj as MultiValueKey;
            if (other != null)
            {
                return this.Equals(other);
            }

            return object.ReferenceEquals(this, obj);
        }

        public bool Equals(MultiValueKey other)
        {
            if (this.Values.Count != other.Values.Count)
            {
                return false;
            }

            return this.Values.All(p => other.Values.ContainsKey(p.Key) && object.Equals(p.Value, other.Values[p.Key]));
        }

        public override int GetHashCode()
        {
            var hash = 0;
            foreach (var item in Values.Keys.OrderBy(p => p))
            {
                hash = hash ^ item.GetHashCode();
            }

            return hash;
        }
    }
}