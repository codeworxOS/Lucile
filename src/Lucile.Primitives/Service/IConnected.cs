namespace Lucile.Service
{
    public interface IConnected<TService>
    {
        TService GetService();
    }
}