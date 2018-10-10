using System.Collections.Generic;
using Lucile.Data.Metadata;

namespace Tests
{
    public class EntityMetadataComparer : IEqualityComparer<EntityMetadata>
    {
        public bool Equals(EntityMetadata x, EntityMetadata y)
        {
            return x.ClrType.Equals(y.ClrType);
        }

        public int GetHashCode(EntityMetadata obj)
        {
            return obj.ClrType.GetHashCode();
        }
    }
}