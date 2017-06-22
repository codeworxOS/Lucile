using System;
using System.ServiceModel.Description;

namespace Lucile.ServiceModel.Behavior
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ErrorHandlerEndpointBehavior : Attribute, IEndpointBehavior
    {
        public void AddBindingParameters(ServiceEndpoint endpoint, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.ClientRuntime clientRuntime)
        {
            clientRuntime.ClientMessageInspectors.Add(new UnWrapExceptionFromFaultInspector());
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.EndpointDispatcher endpointDispatcher)
        {
        }

        public void Validate(ServiceEndpoint endpoint)
        {
            if (!endpoint.Contract.ContractBehaviors.Contains(FaultContractBehavior.Instance))
            {
                endpoint.Contract.ContractBehaviors.Add(FaultContractBehavior.Instance);
            }
        }
    }
}