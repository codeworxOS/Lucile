namespace Lucile.Service
{
    public interface IServiceOptions<TService>
        where TService : class
    {
        ConnectedServiceLifetime Lifetime { get; }
    }
}