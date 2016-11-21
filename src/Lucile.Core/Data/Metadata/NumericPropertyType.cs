using System;

namespace Lucile.Data.Metadata
{
    [Flags]
    public enum NumericPropertyType
    {
        Byte = 0x00,
        Decimal = 0x01,
        Double = 0x02,
        Int16 = 0x04,
        Int32 = 0x08,
        Int64 = 0x10,
        SByte = 0x20,
        Single = 0x40,
        Unsigned = 0x80,
        UInt16 = Int16 | Unsigned,
        UInt32 = Int32 | Unsigned,
        UInt64 = Int64 | Unsigned,
    }
}