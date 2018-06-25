using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Lucile.Dynamic.Convention;
using Lucile.Dynamic.DependencyInjection;
using Lucile.Dynamic.Test.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Lucile.Dynamic.Test
{
    public class ScopeProxyTest
    {
        [Fact]
        public async Task TestAsyncMethodsProxy()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddConnectedService<IAsyncService>();
            serviceCollection.AddConnected<IAsyncService, AsyncService>();
            serviceCollection.AddScoped<ScopedDependency>();
            serviceCollection.AddScopeProxy();

            var prov = serviceCollection.BuildServiceProvider();
            var service = prov.GetRequiredService<IAsyncService>();

            var text = Guid.NewGuid().ToString();
            var call = await service.AsyncMethodWithResult(text, 123);
            Assert.Equal(text, call);

            var ex = await Assert.ThrowsAsync<AsyncService.AsyncServiceException>(() => service.AsyncVoidMethod(text, 1234));
            var ex2 = await Assert.ThrowsAsync<AsyncService.AsyncServiceException>(() => service.AsyncVoidMethod(text, 1234));

            Assert.NotEqual(ex.ScopedDependency.Id, ex2.ScopedDependency.Id);
        }

        [Fact]
        public void TestSyncMethodsProxy()
        {
            var serviceCollection = new ServiceCollection();

            var dtb = new DynamicTypeBuilder<ScopeProxy<ISyncService>>();
            dtb.AddConvention(new ProxyConvention<ISyncService>());

            serviceCollection.AddConnectedService<ISyncService>();
            serviceCollection.AddConnected<ISyncService, SyncService>();
            serviceCollection.AddScoped<ScopedDependency>();
            serviceCollection.AddScopeProxy();

            var prov = serviceCollection.BuildServiceProvider();
            var service = prov.GetRequiredService<ISyncService>();

            var text = Guid.NewGuid().ToString();
            var call = service.SyncMethodWithResult(text, 123);
            Assert.Equal(text, call);

            var ex = Assert.Throws<SyncService.SyncServiceException>(() => service.SyncVoidMethod(text, 1234));
            var ex2 = Assert.Throws<SyncService.SyncServiceException>(() => service.SyncVoidMethod(text, 1234));

            Assert.NotEqual(ex.ScopedDependency.Id, ex2.ScopedDependency.Id);
        }
    }
}