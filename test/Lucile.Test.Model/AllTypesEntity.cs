using System;

namespace Lucile.Test.Model
{
    public class AllTypesEntity : EntityBase
    {
        public byte[] BlobProperty { get; set; }

        public byte ByteProperty { get; set; }

        public DateTimeOffset DateTimeOffsetProperty { get; set; }

        public DateTime DateTimeProperty { get; set; }

        public decimal DecimalProperty { get; set; }

        public double DoubleProperty { get; set; }

        public float FloatProperty { get; set; }

        public int Id { get; set; }

        public int IntProperty { get; set; }

        public long LongProperty { get; set; }

        public byte? NullableByteProperty { get; set; }

        public DateTimeOffset? NullableDateTimeOffsetProperty { get; set; }

        public DateTime? NullableDateTimeProperty { get; set; }

        public decimal? NullableDecimalProperty { get; set; }

        public double? NullableDoubleProperty { get; set; }

        public float? NullableFloatProperty { get; set; }

        public int? NullableIntProperty { get; set; }

        public long? NullableLongProperty { get; set; }

        public sbyte? NullableSByteProperty { get; set; }

        public short? NullableShortProperty { get; set; }

        public uint? NullableUIntProperty { get; set; }

        public ulong? NullableULongProperty { get; set; }

        public ushort? NullableUShortProperty { get; set; }

        public sbyte SByteProperty { get; set; }

        public short ShortProperty { get; set; }

        public string StringProperty { get; set; }

        public uint UIntProperty { get; set; }

        public ulong ULongProperty { get; set; }

        public ushort UShortProperty { get; set; }
    }
}