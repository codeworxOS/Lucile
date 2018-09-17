using System;
using System.Reflection;
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
    public abstract class ScalarPropertyBuilder : IMetadataBuilder
    {
        [DataMember(Order = 3)]
        public bool IsExcluded { get; set; }

        [DataMember(Order = 4)]
        public bool IsIdentity { get; set; }

        [DataMember(Order = 2)]
        public string Name { get; set; }

        [DataMember(Order = 1)]
        public bool Nullable { get; set; }

        public static ScalarPropertyBuilder CreateScalar(PropertyInfo propertyInfo)
        {
            var nullable = System.Nullable.GetUnderlyingType(propertyInfo.PropertyType);

            var type = nullable ?? propertyInfo.PropertyType;
            var numeric = NumericProperty.GetNumericTypeFromClrType(type);

            if (type == typeof(string))
            {
                return new TextPropertyBuilder { Name = propertyInfo.Name, Nullable = true, Unicode = true };
            }
            else if (numeric.HasValue)
            {
                return new NumericPropertyBuilder { Name = propertyInfo.Name, Nullable = nullable != null, NumericType = numeric.Value };
            }
            else if (type == typeof(bool))
            {
                return new BooleanPropertyBuilder { Name = propertyInfo.Name, Nullable = nullable != null };
            }
            else if (type == typeof(byte[]))
            {
                return new BlobPropertyBuilder { Name = propertyInfo.Name, Nullable = true };
            }
            else if (type == typeof(DateTime))
            {
                return new DateTimePropertyBuilder { Name = propertyInfo.Name, Nullable = nullable != null, DateTimeType = DateTimePropertyType.DateTime };
            }
            else if (type == typeof(DateTimeOffset))
            {
                return new DateTimePropertyBuilder { Name = propertyInfo.Name, Nullable = nullable != null, DateTimeType = DateTimePropertyType.DateTimeOffset };
            }
            else if (type == typeof(Guid))
            {
                return new GuidPropertyBuilder { Name = propertyInfo.Name, Nullable = nullable != null };
            }
            else if (type.GetTypeInfo().IsEnum)
            {
                return new EnumPropertyBuilder
                {
                    Name = propertyInfo.Name,
                    Nullable = nullable != null,
                    EnumTypeInfo = new ClrTypeInfo(type),
                    UnderlyingNumericType = NumericProperty.GetNumericTypeFromClrType(Enum.GetUnderlyingType(type)).GetValueOrDefault()
                };
            }

            throw new InvalidOperationException($"PropertyType {type} not supported.");
        }

        public ScalarPropertyBuilder CopyFrom(ScalarPropertyBuilder source)
        {
            this.IsExcluded = source.IsExcluded;
            this.IsIdentity = source.IsIdentity;
            this.Nullable = source.Nullable;

            CopyValues(source);

            return this;
        }

        public ScalarPropertyBuilder CopyFrom(ScalarProperty source)
        {
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

        protected abstract void CopyValues(ScalarProperty source);

        protected abstract ScalarProperty MapToProperty(EntityMetadata entity, bool isPrimaryKey);
    }
}