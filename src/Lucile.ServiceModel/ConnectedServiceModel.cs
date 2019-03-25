using System;
using System.ServiceModel;
using Lucile.Service;

#if NET45
using Microsoft.Extensions.DependencyInjection;
#endif

namespace Lucile.ServiceModel
{
    internal class ConnectedServiceModel<TService> : IDefaultConnected<TService>, IConnected<TService>, IDisposable
        where TService : class
    {
        private readonly RemoteServiceOptions _remoteServiceOptions;
        private readonly TService _service;
        private readonly object _serviceLocker = new object();
        private readonly IServiceProvider _serviceProvider;
        private bool _disposedValue = false;

        public ConnectedServiceModel(IServiceProvider serviceProvider, RemoteServiceOptions remoteServiceOptions)
        {
            _serviceProvider = serviceProvider;
            _remoteServiceOptions = remoteServiceOptions;
            _service = CreateService();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public TService GetService()
        {
            return _service;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (_service is IDisposable disp)
                    {
                        try
                        {
                            disp.Dispose();
                        }
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body -> intended
                        catch (Exception)
                        {
                        }
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
                    }
                }

                _disposedValue = true;
            }
        }

        private TService CreateService()
        {
            var binding = _remoteServiceOptions.GetBinding<TService>(out var callback);
            var endpointAddress = _remoteServiceOptions.GetEndpointAddress<TService>();
            ChannelFactory<TService> cf = null;
            if (callback == null)
            {
                cf = new ChannelFactory<TService>(binding, endpointAddress);
            }
            else
            {
#if NETSTANDARD1_3
                throw new NotSupportedException($"Duplex Services are not supported on net standard");
#else
                var instance = _serviceProvider.GetRequiredService(callback);
                cf = new DuplexChannelFactory<TService>(instance, binding, endpointAddress);
#endif
            }

            _remoteServiceOptions?.OnChannelFactoryAction?.Invoke(_serviceProvider, cf);
            return cf.CreateChannel();
        }
    }
}