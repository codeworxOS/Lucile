using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Lucile.Dynamic.Convention;
using Lucile.Dynamic.DependencyInjection;
using Lucile.Dynamic.DependencyInjection.Service;
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
            serviceCollection.AddConnectedLocal<IAsyncService, AsyncService>();
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
        public async Task TestAsyncMethodsServiceModelProxy()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddConnectedService<IAsyncService>();
            serviceCollection.UseRemoteServices(builder => builder.Base("net.tcp://localhost:1234/whatever"));
            serviceCollection.AddScoped<ScopedDependency>();
            serviceCollection.AddScopeProxy();

            var prov = serviceCollection.BuildServiceProvider();
            var service = prov.GetRequiredService<IAsyncService>();

            var text = Guid.NewGuid().ToString();

            await Assert.ThrowsAsync<EndpointNotFoundException>(() => service.IsAliveAsync());
            var ex = await Assert.ThrowsAsync<EndpointNotFoundException>(() => service.AsyncMethodWithResult(text, 1234));
            var ex2 = await Assert.ThrowsAsync<EndpointNotFoundException>(() => service.AsyncVoidMethod(text, 1234));
        }

        [Fact]
        public async Task TestAsyncProxyMiddlware()
        {
            AsyncMiddlewareContext proxyContext = null;

            var middleware = new DelegateAsyncProxyMiddlware(p => proxyContext = proxyContext ?? p);

            var serviceCollection = new ServiceCollection();

            serviceCollection.AddConnectedService<IAsyncService>();
            serviceCollection.AddConnectedLocal<IAsyncService, AsyncService>();
            serviceCollection.AddScoped<ScopedDependency>();
            serviceCollection.AddScopeProxy();
            serviceCollection.AddSingleton<IAsyncProxyMiddleware>(middleware);

            var prov = serviceCollection.BuildServiceProvider();
            var service = prov.GetRequiredService<IAsyncService>();

            var text = Guid.NewGuid().ToString();
            var call = await service.AsyncMethodWithResult(text, 123);

            Assert.NotNull(proxyContext);
            Assert.Equal(text, proxyContext.InterceptionContext.Arguments[0]);
            Assert.Equal(123, proxyContext.InterceptionContext.Arguments[1]);
            Assert.Equal(nameof(IAsyncService.AsyncMethodWithResult), proxyContext.InterceptionContext.MemberName);

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
            serviceCollection.AddConnectedLocal<ISyncService, SyncService>();
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

        [Fact]
        public void TestSyncMethodsServiceModelProxy()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddConnectedService<ISyncService>();
            serviceCollection.UseRemoteServices(builder => builder.Base("net.tcp://localhost:1234/whatever"));
            serviceCollection.AddScoped<ScopedDependency>();
            serviceCollection.AddScopeProxy();

            var prov = serviceCollection.BuildServiceProvider();
            var service = prov.GetRequiredService<ISyncService>();

            var text = Guid.NewGuid().ToString();

            var ex = Assert.Throws<EndpointNotFoundException>(() => service.SyncMethodWithResult(text, 1234));
            var ex2 = Assert.Throws<EndpointNotFoundException>(() => service.SyncVoidMethod(text, 1234));
        }

        [Fact]
        public void TestSyncProxyMiddlware()
        {
            SyncMiddlewareContext proxyContext = null;

            var middleware = new DelegateSyncProxyMiddlware(p => proxyContext = proxyContext ?? p);

            var serviceCollection = new ServiceCollection();

            serviceCollection.AddConnectedService<ISyncService>();
            serviceCollection.AddConnectedLocal<ISyncService, SyncService>();
            serviceCollection.AddScoped<ScopedDependency>();
            serviceCollection.AddScopeProxy();
            serviceCollection.AddSingleton<ISyncProxyMiddleware>(middleware);

            var prov = serviceCollection.BuildServiceProvider();
            var service = prov.GetRequiredService<ISyncService>();

            var text = Guid.NewGuid().ToString();
            var call = service.SyncMethodWithResult(text, 123);

            Assert.NotNull(proxyContext);
            Assert.Equal(text, proxyContext.InterceptionContext.Arguments[0]);
            Assert.Equal(123, proxyContext.InterceptionContext.Arguments[1]);
            Assert.Equal(nameof(ISyncService.SyncMethodWithResult), proxyContext.InterceptionContext.MemberName);

            var ex = Assert.Throws<SyncService.SyncServiceException>(() => service.SyncVoidMethod(text, 1234));
            var ex2 = Assert.Throws<SyncService.SyncServiceException>(() => service.SyncVoidMethod(text, 1234));

            Assert.NotEqual(ex.ScopedDependency.Id, ex2.ScopedDependency.Id);
        }
    }
}