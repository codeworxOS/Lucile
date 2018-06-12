using System.ServiceModel;
using Microsoft.Extensions.DependencyInjection;

namespace Lucile.ServiceModel.DependencyInjection.Behavior
{
    public class ServiceScopeExtension : IExtension<InstanceContext>
    {
        public ServiceScopeExtension(IServiceScope scope)
        {
            Scope = scope;
        }

        public IServiceScope Scope { get; }

        public void Attach(InstanceContext owner)
        {
        }

        public void Detach(InstanceContext owner)
        {
        }
    }
}