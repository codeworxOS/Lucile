namespace Lucile.Dynamic.DependencyInjection.Service
{
    public interface ISyncProxyMiddleware
    {
        void Invoke(SyncProxyDelegate next, SyncMiddlewareContext context);
    }
}