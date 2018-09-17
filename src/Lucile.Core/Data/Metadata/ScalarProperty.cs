using Lucile.Data.Metadata.Builder;

namespace Lucile.Data.Metadata
{
    public abstract class ScalarProperty : PropertyMetadata
    {
        public ScalarProperty(EntityMetadata enity, ScalarPropertyBuilder scalarBuilder, bool isPrimaryKey)
                : base(enity, scalarBuilder)
        {
            ValueGeneration = scalarBuilder.ValueGeneration;
            IsPrimaryKey = isPrimaryKey;
        }

        public bool IsPrimaryKey { get; }

        public AutoGenerateValue ValueGeneration { get; }
    }
}