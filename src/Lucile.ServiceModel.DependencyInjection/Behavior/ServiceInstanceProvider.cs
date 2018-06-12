using System;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using Microsoft.Extensions.DependencyInjection;

namespace Lucile.ServiceModel.DependencyInjection.Behavior
{
    internal class ServiceInstanceProvider : IInstanceProvider
    {
        private readonly IServiceProvider _serviceProvider;

        public ServiceInstanceProvider(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }

        public object GetInstance(InstanceContext instanceContext)
        {
            var service = instanceContext.Host.Description.ServiceType;

            var scope = _serviceProvider.CreateScope();
            instanceContext.Extensions.Add(new ServiceScopeExtension(scope));
            var messageScopeExtension = instanceContext.Extensions.Find<MessageScopeExtension>();
            if (messageScopeExtension != null)
            {
                messageScopeExtension.Collection.Register(scope.ServiceProvider);
            }

            var instance = scope.ServiceProvider.GetService(service);
            return instance;
        }

        public object GetInstance(InstanceContext instanceContext, Message message)
        {
            return GetInstance(instanceContext);
        }

        public void ReleaseInstance(InstanceContext instanceContext, object instance)
        {
            var scope = instanceContext.Extensions.OfType<ServiceScopeExtension>().FirstOrDefault();
            if (scope != null)
            {
                scope.Scope.Dispose();
            }
        }
    }
}