using System.Runtime.Serialization;
using ProtoBuf;

namespace Lucile.Data.Metadata.Builder
{
    [DataContract(IsReference = true)]
    [KnownType(typeof(TextPropertyBuilder))]
    [KnownType(typeof(NumericPropertyBuilder))]
    [KnownType(typeof(BooleanPropertyBuilder))]
    [KnownType(typeof(BlobPropertyBuilder))]
    [KnownType(typeof(DateTimePropertyBuilder))]
    [KnownType(typeof(GuidPropertyBuilder))]
    [KnownType(typeof(GeometryPropertyBuilder))]
    [KnownType(typeof(GeographyPropertyBuilder))]
    [KnownType(typeof(EnumPropertyBuilder))]
    [ProtoInclude(101, typeof(TextPropertyBuilder))]
    [ProtoInclude(102, typeof(NumericPropertyBuilder))]
    [ProtoInclude(103, typeof(BooleanPropertyBuilder))]
    [ProtoInclude(104, typeof(BlobPropertyBuilder))]
    [ProtoInclude(105, typeof(DateTimePropertyBuilder))]
    [ProtoInclude(106, typeof(GuidPropertyBuilder))]
    [ProtoInclude(107, typeof(GeometryPropertyBuilder))]
    [ProtoInclude(108, typeof(GeographyPropertyBuilder))]
    [ProtoInclude(109, typeof(EnumPropertyBuilder))]
    public abstract class ScalarPropertyBuilder : PropertyMetadataBuilder
    {
        [DataMember(Order = 1)]
        public bool IsIdentity { get; set; }

        public ScalarPropertyBuilder CopyFrom(ScalarPropertyBuilder source)
        {
            this.IsExcluded = source.IsExcluded;
            this.IsIdentity = source.IsIdentity;
            this.Nullable = source.Nullable;

            CopyValues(source);

            return this;
        }

        internal ScalarProperty ToProperty(EntityMetadata entity, bool isPrimaryKey)
        {
            return MapToProperty(entity, isPrimaryKey);
        }

        protected abstract void CopyValues(ScalarPropertyBuilder source);

        protected abstract ScalarProperty MapToProperty(EntityMetadata entity, bool isPrimaryKey);
    }
}