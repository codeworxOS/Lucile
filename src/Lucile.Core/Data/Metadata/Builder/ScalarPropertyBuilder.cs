using System;
using System.Reflection;
using System.Runtime.Serialization;
using Lucile.Json;
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
    [KnownType(typeof(TimeSpanPropertyBuilder))]
    [ProtoInclude(101, typeof(TextPropertyBuilder))]
    [ProtoInclude(102, typeof(NumericPropertyBuilder))]
    [ProtoInclude(103, typeof(BooleanPropertyBuilder))]
    [ProtoInclude(104, typeof(BlobPropertyBuilder))]
    [ProtoInclude(105, typeof(DateTimePropertyBuilder))]
    [ProtoInclude(106, typeof(GuidPropertyBuilder))]
    [ProtoInclude(107, typeof(GeometryPropertyBuilder))]
    [ProtoInclude(108, typeof(GeographyPropertyBuilder))]
    [ProtoInclude(109, typeof(EnumPropertyBuilder))]
    [ProtoInclude(110, typeof(TimeSpanPropertyBuilder))]
    [JsonConverter(typeof(JsonInheritanceConverter), "type")]
    public abstract class ScalarPropertyBuilder : IMetadataBuilder
    {
        [DataMember(Order = 5)]
        public bool HasDefaultValue { get; set; }

        [DataMember(Order = 3)]
        public bool IsExcluded { get; set; }

        [DataMember(Order = 2)]
        public string Name { get; set; }

        [DataMember(Order = 1)]
        public bool Nullable { get; set; }

        [DataMember(Order = 4)]
        public AutoGenerateValue ValueGeneration { get; set; }

        [DataMember(Order = 6)]
        public ClrTypeInfo PropertyType { get; set; }

        public static ScalarPropertyBuilder CreateScalar(PropertyInfo propertyInfo)
        {
            return CreateScalar(propertyInfo.Name, propertyInfo.PropertyType);
        }

        public static ScalarPropertyBuilder CreateScalar(string propertyName, Type propertyType)
        {
            var nullable = System.Nullable.GetUnderlyingType(propertyType);

            var typeInfo = new ClrTypeInfo(propertyType);

            var type = nullable ?? propertyType;
            var numeric = NumericProperty.GetNumericTypeFromClrType(type);

            if (type == typeof(string))
            {
                return new TextPropertyBuilder { PropertyType = typeInfo, Name = propertyName, Nullable = true, Unicode = true };
            }
            else if (numeric.HasValue)
            {
                return new NumericPropertyBuilder { PropertyType = typeInfo, Name = propertyName, Nullable = nullable != null, NumericType = numeric.Value };
            }
            else if (type == typeof(bool))
            {
                return new BooleanPropertyBuilder { PropertyType = typeInfo, Name = propertyName, Nullable = nullable != null };
            }
            else if (type == typeof(byte[]))
            {
                return new BlobPropertyBuilder { PropertyType = typeInfo, Name = propertyName, Nullable = true };
            }
            else if (type == typeof(DateTime))
            {
                return new DateTimePropertyBuilder { PropertyType = typeInfo, Name = propertyName, Nullable = nullable != null, DateTimeType = DateTimePropertyType.DateTime };
            }
            else if (type == typeof(TimeSpan))
            {
                return new TimeSpanPropertyBuilder { PropertyType = typeInfo, Name = propertyName, Nullable = nullable != null };
            }
            else if (type == typeof(DateTimeOffset))
            {
                return new DateTimePropertyBuilder { PropertyType = typeInfo, Name = propertyName, Nullable = nullable != null, DateTimeType = DateTimePropertyType.DateTimeOffset };
            }
            else if (type == typeof(Guid))
            {
                return new GuidPropertyBuilder { PropertyType = typeInfo, Name = propertyName, Nullable = nullable != null };
            }
            else if (type.GetTypeInfo().IsEnum)
            {
                return new EnumPropertyBuilder
                {
                    Name = propertyName,
                    PropertyType = typeInfo,
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
            this.ValueGeneration = source.ValueGeneration;
            this.Nullable = source.Nullable;
            this.PropertyType = source.PropertyType;

            CopyValues(source);

            return this;
        }

        public ScalarPropertyBuilder CopyFrom(ScalarProperty source)
        {
            this.ValueGeneration = source.ValueGeneration;
            this.Nullable = source.Nullable;
            this.PropertyType = new ClrTypeInfo(source.PropertyType);
            this.HasDefaultValue = source.HasDefaultValue;

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