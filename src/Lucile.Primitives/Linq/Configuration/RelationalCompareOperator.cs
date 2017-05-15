namespace Lucile.Linq.Configuration
{
    public enum RelationalCompareOperator
    {
        Equal = 0x00,
        NotEqual = 0x01,
        GreaterThen = 0x02,
        GreaterThenOrEqual = 0x04,
        LessThen = 0x08,
        LessThenOrEqual = 0x10,
        IsNull = 0x20,
        IsNotNull = 0x40
    }
}