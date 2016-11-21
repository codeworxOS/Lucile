namespace Lucile.Linq.Configuration
{
    public enum StringOperator
    {
        Equal = 0x00,
        NotEqual = 0x01,
        Contains = 0x20,
        StartsWith = 0x40,
        EndsWith = 0x80
    }
}