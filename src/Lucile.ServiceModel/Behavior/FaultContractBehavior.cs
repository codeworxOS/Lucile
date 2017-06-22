using System;
using System.Linq;
using System.ServiceModel.Description;

namespace Lucile.ServiceModel.Behavior
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    public class FaultContractBehavior : Attribute, IContractBehavior
    {
        private static readonly FaultContractBehavior _instance = new FaultContractBehavior();

        private FaultContractBehavior()
        {
        }

        public static FaultContractBehavior Instance => _instance;

        public static void MakeFault(OperationDescription op)
        {
            if (!op.Faults.Any(p => p.DetailType == typeof(ExceptionFault)))
            {
                FaultDescription fd = new FaultDescription(op.Name);
                fd.DetailType = typeof(ExceptionFault);
                op.Faults.Add(fd);
            }
        }

        public void AddBindingParameters(ContractDescription contractDescription, ServiceEndpoint endpoint, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.ClientRuntime clientRuntime)
        {
        }

        public void ApplyDispatchBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.DispatchRuntime dispatchRuntime)
        {
        }

        public void Validate(
                ContractDescription contractDescription,
                ServiceEndpoint endpoint)
        {
            foreach (OperationDescription op in contractDescription.Operations)
            {
                MakeFault(op);
            }
        }
    }
}