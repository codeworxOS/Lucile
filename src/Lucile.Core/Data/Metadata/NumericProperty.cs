using System;
using System.Collections.Immutable;
using System.Linq;
using Lucile.Data.Metadata.Builder;

namespace Lucile.Data.Metadata
{
    public class NumericProperty : ScalarProperty
    {
        private static readonly ImmutableDictionary<Type, NumericPropertyType> _numericTypes = ImmutableDictionary.Create<Type, NumericPropertyType>()
            .Add(typeof(byte), NumericPropertyType.Byte)
            .Add(typeof(sbyte), NumericPropertyType.SByte)
            .Add(typeof(int), NumericPropertyType.Int32)
            .Add(typeof(uint), NumericPropertyType.UInt32)
            .Add(typeof(long), NumericPropertyType.Int64)
            .Add(typeof(ulong), NumericPropertyType.UInt64)
            .Add(typeof(short), NumericPropertyType.Int16)
            .Add(typeof(ushort), NumericPropertyType.UInt16)
            .Add(typeof(float), NumericPropertyType.Single)
            .Add(typeof(double), NumericPropertyType.Double)
            .Add(typeof(decimal), NumericPropertyType.Decimal);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="enity"></param>
        internal NumericProperty(EntityMetadata enity, NumericPropertyBuilder builder, bool isPrimaryKey)
            : base(enity, builder, isPrimaryKey)
        {
            NumericPropertyType = builder.NumericType;
            if (builder.Precision > 0)
            {
                Precision = builder.Precision;
            }

            if (builder.Scale > 0)
            {
                Scale = builder.Scale;
            }
        }

        public NumericPropertyType NumericPropertyType { get; }

        public byte? Precision { get; }

        public byte? Scale { get; }

        public static Type GetClrTypeFromNumericType(NumericPropertyType numericType)
        {
            return _numericTypes.Where(p => p.Value == numericType).Select(p => p.Key).FirstOrDefault();
        }

        public static NumericPropertyType? GetNumericTypeFromClrType(Type clrType)
        {
            NumericPropertyType value;
            if (_numericTypes.TryGetValue(clrType, out value))
            {
                return value;
            }

            return null;
        }
    }
}