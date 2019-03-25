namespace Lucile.Service
{
    internal interface ILocal<TService>
        where TService : class
    {
        IConnected<TService> GetConnectedService();
    }
}