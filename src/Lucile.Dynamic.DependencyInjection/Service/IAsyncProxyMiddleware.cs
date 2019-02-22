using System.Threading.Tasks;

namespace Lucile.Dynamic.DependencyInjection.Service
{
    public interface IAsyncProxyMiddleware
    {
        Task Invoke(AsyncProxyDelegate next, AsyncMiddlewareContext context);
    }
}