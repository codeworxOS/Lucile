namespace Lucile.Linq.Configuration
{
    public enum StringOperator
    {
        Equal = 0x00,
        NotEqual = 0x01,
        Contains = 0x02,
        StartsWith = 0x04,
        EndsWith = 0x08,
        IsNull = 0x20,
        IsNotNull = 0x40
    }
}