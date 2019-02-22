using System.Threading.Tasks;

namespace Lucile.Dynamic.DependencyInjection.Service
{
    public delegate Task AsyncProxyDelegate(AsyncMiddlewareContext context);
}