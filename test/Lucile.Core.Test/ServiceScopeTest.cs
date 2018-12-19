using System;
using System.Collections.Generic;
using System.Text;
using Lucile.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Tests
{
    public class ServiceScopeTest
    {
        [Fact]
        public void ServiceScopeWithInterceptorTest()
        {
            var services = new ServiceCollection();

            services.AddScoped<ScopedInfo>();
            services.AddTransient<IServiceScopeInterceptor, ScopedInfoInterceptor>();

            var id = Guid.NewGuid();

            using (var sp = services.BuildServiceProvider())
            {
                var info = sp.GetService<ScopedInfo>();
                info.Id = id;

                using (var scope = sp.CreateScope(true))
                {
                    var scopeInfo = scope.ServiceProvider.GetService<ScopedInfo>();

                    Assert.NotEqual(scopeInfo, info);
                    Assert.Equal(id, scopeInfo.Id);
                }
            }
        }

        [Fact]
        public void ServiceScopeWithoutInterceptorTest()
        {
            var services = new ServiceCollection();

            services.AddScoped<ScopedInfo>();
            services.AddTransient<IServiceScopeInterceptor, ScopedInfoInterceptor>();

            var id = Guid.NewGuid();

            using (var sp = services.BuildServiceProvider())
            {
                var info = sp.GetService<ScopedInfo>();
                info.Id = id;

                using (var scope = sp.CreateScope(false))
                {
                    var scopeInfo = scope.ServiceProvider.GetService<ScopedInfo>();

                    Assert.NotEqual(scopeInfo, info);
                    Assert.Equal(Guid.Empty, scopeInfo.Id);
                }
            }
        }

        private class ScopedInfo
        {
            public Guid Id { get; set; }
        }

        private class ScopedInfoInterceptor : IServiceScopeInterceptor
        {
            public void ScopeCreated(IServiceProvider parent, IServiceScope child)
            {
                var parentInfo = parent.GetService<ScopedInfo>();

                var childInfo = child.ServiceProvider.GetService<ScopedInfo>();
                childInfo.Id = parentInfo.Id;
            }
        }
    }
}