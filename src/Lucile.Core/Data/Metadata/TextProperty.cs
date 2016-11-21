using Lucile.Data.Metadata.Builder;

namespace Lucile.Data.Metadata
{
    public class TextProperty : ScalarProperty
    {
        internal TextProperty(EntityMetadata enity, TextPropertyBuilder builder, bool isPrimaryKey)
            : base(enity, builder, isPrimaryKey)
        {
            Unicode = builder.Unicode;
            MaxLength = builder.MaxLength;
            IsFixedLength = builder.FixedLength;
        }

        public bool IsFixedLength { get; }

        public int? MaxLength { get; }

        public bool Unicode { get; }
    }
}