namespace Lucile.Service
{
    internal interface ILocal<TService>
    {
        IConnected<TService> GetConnectedService();
    }
}