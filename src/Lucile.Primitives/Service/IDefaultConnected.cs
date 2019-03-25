namespace Lucile.Service
{
    public interface IDefaultConnected<TService> : IConnected<TService>
        where TService : class
    {
    }
}