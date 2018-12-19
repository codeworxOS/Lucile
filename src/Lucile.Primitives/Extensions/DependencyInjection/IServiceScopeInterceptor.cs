using System;
using Microsoft.Extensions.DependencyInjection;

namespace Lucile.Extensions.DependencyInjection
{
    public interface IServiceScopeInterceptor
    {
        void ScopeCreated(IServiceProvider parent, IServiceScope child);
    }
}