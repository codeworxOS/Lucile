using System;
using System.Runtime.Serialization;

namespace Lucile.Data.Metadata
{
    [DataContract]
    [ProtoBuf.ProtoInclude(200, typeof(MultiValueKey))]
    [KnownType(typeof(MultiValueKey))]
    public class EntityKey : EntityTypeObject, IEquatable<EntityKey>
    {
        [DataMember(Order = 1)]
        public object Key { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as EntityKey;
            if (other != null)
            {
                return this.Equals(other);
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return (this.EntityType == null ? 0 : this.EntityType.GetHashCode()) ^ (this.Key == null ? 0 : this.Key.GetHashCode());
        }

        public bool Equals(EntityKey other)
        {
            return object.Equals(this.EntityType, other.EntityType) & object.Equals(this.Key, other.Key);
        }
    }
}