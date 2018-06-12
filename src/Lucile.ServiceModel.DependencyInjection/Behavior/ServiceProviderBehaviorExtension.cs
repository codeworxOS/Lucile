using System;
using System.ServiceModel.Configuration;

namespace Lucile.ServiceModel.DependencyInjection.Behavior
{
    public class ServiceProviderBehaviorExtension : BehaviorExtensionElement
    {
        public override Type BehaviorType => typeof(ServiceProviderBehavior);

        protected override object CreateBehavior()
        {
            return new ServiceProviderBehavior();
        }
    }
}