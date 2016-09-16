using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Lucile.Data.Metadata
{
    /// <summary>
    /// Funktionalität Metadaten für ein skalares Property
    /// </summary>
    [DataContract(IsReference = true)]
    [ProtoBuf.ProtoContract(AsReferenceDefault = true)]
    [KnownType(typeof(BlobProperty))]
    [KnownType(typeof(BooleanProperty))]
    [KnownType(typeof(NumericProperty))]
    [KnownType(typeof(TextProperty))]
    [KnownType(typeof(DateTimeProperty))]
    [KnownType(typeof(GuidProperty))]
    [KnownType(typeof(EnumProperty))]
    [KnownType(typeof(GeometryProperty))]
    [KnownType(typeof(GeographyProperty))]
    [ProtoBuf.ProtoInclude(110, typeof(BlobProperty))]
    [ProtoBuf.ProtoInclude(111, typeof(BooleanProperty))]
    [ProtoBuf.ProtoInclude(112, typeof(NumericProperty))]
    [ProtoBuf.ProtoInclude(113, typeof(TextProperty))]
    [ProtoBuf.ProtoInclude(114, typeof(DateTimeProperty))]
    [ProtoBuf.ProtoInclude(115, typeof(GuidProperty))]
    [ProtoBuf.ProtoInclude(116, typeof(EnumProperty))]
    [ProtoBuf.ProtoInclude(117, typeof(GeometryProperty))]
    [ProtoBuf.ProtoInclude(118, typeof(GeographyProperty))]
    public abstract class ScalarProperty : PropertyMetadata
    {
        private static readonly ImmutableDictionary<Type, NumericPropertyType> _numericTypes = ImmutableDictionary.Create<Type, NumericPropertyType>()
            .Add(typeof(byte), NumericPropertyType.Byte)
            .Add(typeof(sbyte), NumericPropertyType.SByte)
            .Add(typeof(int), NumericPropertyType.Int32)
            .Add(typeof(uint), NumericPropertyType.Int32)
            .Add(typeof(long), NumericPropertyType.Int64)
            .Add(typeof(ulong), NumericPropertyType.Int64)
            .Add(typeof(short), NumericPropertyType.Int16)
            .Add(typeof(ushort), NumericPropertyType.Int16)
            .Add(typeof(float), NumericPropertyType.Single)
            .Add(typeof(double), NumericPropertyType.Double)
            .Add(typeof(decimal), NumericPropertyType.Decimal);

        public ScalarProperty(EntityMetadata enity)
                : base(enity)
        {
        }

        protected ScalarProperty()
        {
        }

        [DataMember(Order = 2)]
        public bool IsIdentity { get; set; }

        [DataMember(Order = 1)]
        public bool IsPrimaryKey { get; set; }

        public static ScalarProperty Create(EntityMetadata entity, PropertyInfo property)
        {
            var type = System.Nullable.GetUnderlyingType(property.PropertyType);
            bool isNullable = false;

            if (type == null)
            {
                type = property.PropertyType;
            }
            else
            {
                isNullable = true;
            }

            if (type == typeof(string))
            {
                return new TextProperty(entity) { Name = property.Name };
            }
            else if (_numericTypes.ContainsKey(type))
            {
                return new NumericProperty(entity)
                {
                    Name = property.Name,
                    Nullable = isNullable,
                    NumericPropertyType = _numericTypes[type]
                };
            }
            else if (type == typeof(bool))
            {
                return new BooleanProperty(entity)
                {
                    Name = property.Name,
                    Nullable = isNullable
                };
            }
            else if (type == typeof(byte[]))
            {
                return new BlobProperty(entity)
                {
                    Name = property.Name
                };
            }
            else if (type == typeof(DateTime) || type == typeof(DateTimeOffset))
            {
                return new DateTimeProperty(entity)
                {
                    Name = property.Name,
                    DateTimePropertyType = type == typeof(DateTimeOffset) ? DateTimePropertyType.DateTimeOffset : DateTimePropertyType.DateTime
                };
            }
            else if (type == typeof(Guid))
            {
                return new GuidProperty(entity)
                {
                    Name = property.Name,
                    Nullable = isNullable
                };
            }
            else if (type.GetTypeInfo().IsEnum)
            {
                return new EnumProperty(entity)
                {
                    Name = property.Name,
                    Nullable = isNullable,
                    EnumType = type,
                    UnderlyingPrimitiveType = Enum.GetUnderlyingType(type),
                    IsFlag = type.GetTypeInfo().CustomAttributes.OfType<FlagsAttribute>().Any()
                };
            }

            throw new NotSupportedException($"The Type {property.PropertyType} is not a valid scalar property type");
            ////else if (type == typeof(DbGeometry))
            ////{
            ////    return new GeometryProperty(entity)
            ////    {
            ////        Name = property.Name,
            ////        Nullable = isNullable
            ////    };
            ////}
            ////else if (type.GetTypeInfo().IsEnum)
            ////{
            ////    return new GeographyProperty(entity)
            ////    {
            ////        Name = property.Name,
            ////        Nullable = isNullable,

            ////    };
            ////}
        }
    }
}