using System;
using System.Reflection;
using System.Runtime.Serialization;
using ProtoBuf;

namespace Lucile.Data.Metadata.Builder
{
    [DataContract(IsReference = true)]
    [KnownType(typeof(ScalarPropertyBuilder))]
    [KnownType(typeof(NavigationPropertyBuilder))]
    [ProtoInclude(101, typeof(ScalarPropertyBuilder))]
    [ProtoInclude(102, typeof(NavigationPropertyBuilder))]
    public abstract class PropertyMetadataBuilder
    {
        [DataMember(Order = 1)]
        public string Name { get; set; }

        [DataMember(Order = 2)]
        public bool Nullable { get; set; }

        public static ScalarPropertyBuilder CreateScalar(PropertyInfo propertyInfo)
        {
            var nullable = System.Nullable.GetUnderlyingType(propertyInfo.PropertyType);

            var type = nullable ?? propertyInfo.PropertyType;
            var numeric = NumericProperty.GetNumericTypeFromClrType(type);

            if (type == typeof(string))
            {
                return new TextPropertyBuilder { Name = propertyInfo.Name };
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
                return new BlobPropertyBuilder { Name = propertyInfo.Name };
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
    }
}