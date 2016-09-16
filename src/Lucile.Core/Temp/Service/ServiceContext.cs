using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Codeworx.ComponentModel;
using Codeworx.Service;
using System.Threading.Tasks;
using System.Threading;
using Codeworx.Dynamic;
using Codeworx.Reflection;

namespace Codeworx.Core.Service
{
    public class ServiceContext
    {
        private ConcurrentDictionary<Type, IServiceConnectionFactory<object>> factories;

        public IFactoryProvider DefaultFactoryProvider { get; private set; }

        private ServiceContext()
        {
            this.factories = new ConcurrentDictionary<Type, IServiceConnectionFactory<object>>();

#if (!SILVERLIGHT)
            Type type = TypeResolver.GetType(Properties.Settings.Default.DefaultServiceFactoryProvider);
            if (type != null)
                this.DefaultFactoryProvider = Activator.CreateInstance(type) as IFactoryProvider;
#endif
        }

        public void RegisterFactory<TChannel>(IServiceConnectionFactory<TChannel> factory) where TChannel : class
        {
            this.factories.AddOrUpdate(typeof(TChannel), (IServiceConnectionFactory<object>)factory, (p, q) => (IServiceConnectionFactory<object>)factory);
        }

        public void SetDefaultFactoryProvider(IFactoryProvider factoryProvider)
        {
            if (factoryProvider == null)
                throw new ArgumentNullException("factoryProvider");

            this.DefaultFactoryProvider = factoryProvider;
        }

        public TChannel GetService<TChannel>(ChannelProxy<TChannel> proxy, CancellationToken cancellationToken = default(CancellationToken)) where TChannel : class
        {
            var factory = GetFactory<TChannel>();
            if (proxy == null)
            {
                proxy = new ChannelProxyFactory<TChannel, ChannelProxy<TChannel>>().GetProxy();
            }
            TChannel channel = null;

            bool wrap = true;

            using (var scope = new ChannelCreationScope(proxy))
            {
                channel = factory.GetChannel(cancellationToken);
                wrap = !scope.DisableProxyWrapping;
            }
            if (wrap)
            {
                ((IDynamicProxy)proxy).SetProxyTarget<TChannel>(channel);
                return proxy as TChannel;
            }
            return channel;
        }

        public TChannel GetService<TChannel>(CancellationToken cancellationToken = default(CancellationToken)) where TChannel : class
        {
            return GetService<TChannel>(null, cancellationToken);
        }

        private IServiceConnectionFactory<TChannel> GetFactory<TChannel>() where TChannel : class
        {
            var factory = this.factories.Values.OfType<IServiceConnectionFactory<TChannel>>().FirstOrDefault();
            if (factory == null)
            {
                factory = this.DefaultFactoryProvider.GetFactory<TChannel>();
            }
            return factory;
        }

        public async Task<TChannel> GetServiceAsync<TChannel>(ChannelProxy<TChannel> proxy, CancellationToken cancellationToken = default(CancellationToken)) where TChannel : class
        {
            return await Task.Factory.StartNew<TChannel>(p => GetService<TChannel>(proxy, (CancellationToken)p), cancellationToken, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public async Task<TChannel> GetServiceAsync<TChannel>(CancellationToken cancellationToken = default(CancellationToken)) where TChannel : class
        {
            return await Task.Factory.StartNew<TChannel>(p => GetService<TChannel>((CancellationToken)p), cancellationToken, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        private static ServiceContext current;

        public static ServiceContext Current
        {
            get
            {
                if (current == null)
                    current = new ServiceContext();

                return current;
            }
        }
    }
}
