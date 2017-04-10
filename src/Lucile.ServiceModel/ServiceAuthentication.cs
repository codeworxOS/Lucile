namespace Lucile.ServiceModel
{
    public enum ServiceAuthentication
    {
        None = 0x00,
        Windows = 0x01,
        UsernamePassword = 0x02,
        Certificate = 0x04
    }
}