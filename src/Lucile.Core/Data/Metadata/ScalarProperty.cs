using Lucile.Data.Metadata.Builder;

namespace Lucile.Data.Metadata
{
    public abstract class ScalarProperty : PropertyMetadata
    {
        public ScalarProperty(EntityMetadata enity, ScalarPropertyBuilder scalarBuilder, bool isPrimaryKey)
                : base(enity, scalarBuilder)
        {
            IsIdentity = scalarBuilder.IsIdentity;
            IsPrimaryKey = isPrimaryKey;
        }

        public bool IsIdentity { get; }

        public bool IsPrimaryKey { get; }
    }
}