using Lucile.Data.Metadata.Builder;

namespace Lucile.Data.Metadata
{
    public abstract class ScalarProperty : PropertyMetadata, IScalarProperty
    {
        public ScalarProperty(EntityMetadata enity, ScalarPropertyBuilder scalarBuilder, bool isPrimaryKey)
                : base(enity, scalarBuilder)
        {
            ValueGeneration = scalarBuilder.ValueGeneration;
            HasDefaultValue = scalarBuilder.HasDefaultValue;
            IsPrimaryKey = isPrimaryKey;
        }

        public bool HasDefaultValue { get; }

        public bool IsPrimaryKey { get; }

        public AutoGenerateValue ValueGeneration { get; }
    }
}