namespace Lucile.Service
{
    public class ServiceOptions<TService> : IServiceOptions<TService>
        where TService : class
    {
        public ServiceOptions(ConnectedServiceLifetime lifetime)
        {
            Lifetime = lifetime;
        }

        public ConnectedServiceLifetime Lifetime { get; }
    }
}