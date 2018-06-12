using System;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using Microsoft.Extensions.DependencyInjection;

namespace Lucile.ServiceModel.DependencyInjection.Behavior
{
    internal class ServiceProviderMessageInspector : IDispatchMessageInspector
    {
        private readonly IServiceProvider _provider;

        public ServiceProviderMessageInspector(IServiceProvider provider)
        {
            this._provider = provider;
        }

        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            var collection = new MessageScopeCollection();
            var scopeInitializers = _provider.GetServices<IMessageScopeInitializer>();

            foreach (var initializer in scopeInitializers)
            {
                collection.Add(initializer.Initialize(ref request));
            }

            var scopeExtension = instanceContext.Extensions.OfType<ServiceScopeExtension>().FirstOrDefault();

            var ext = instanceContext.Extensions.OfType<MessageScopeExtension>().FirstOrDefault();
            if (ext == null)
            {
                instanceContext.Extensions.Remove(ext);
            }

            instanceContext.Extensions.Add(new MessageScopeExtension(collection));

            if (scopeExtension != null)
            {
                collection.Register(scopeExtension.Scope.ServiceProvider);
            }

            return collection;
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            var disp = correlationState as IDisposable;
            disp?.Dispose();
        }
    }
}