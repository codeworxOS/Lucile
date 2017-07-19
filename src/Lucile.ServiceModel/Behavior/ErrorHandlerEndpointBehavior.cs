using System;
using System.ServiceModel.Description;

namespace Lucile.ServiceModel.Behavior
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ErrorHandlerEndpointBehavior : Attribute, IEndpointBehavior
    {
        public ErrorHandlerEndpointBehavior()
        {
        }

#if NET45
        public ErrorHandlerEndpointBehavior(Func<Exception, string> logDelegate, bool includeDetails = false)
        {
            LogDelegate = logDelegate;
            IncludeDetails = false;
        }
#endif

        public bool IncludeDetails { get; set; }

        public Func<Exception, string> LogDelegate { get; set; }

        public void AddBindingParameters(ServiceEndpoint endpoint, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.ClientRuntime clientRuntime)
        {
            clientRuntime.ClientMessageInspectors.Add(new UnWrapExceptionFromFaultInspector());
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.EndpointDispatcher endpointDispatcher)
        {
#if NET45
            endpointDispatcher.ChannelDispatcher.ErrorHandlers.Add(new SerializeExceptionErrorHandler(this.LogDelegate, this.IncludeDetails));
#endif
        }

        public void Validate(ServiceEndpoint endpoint)
        {
            if (!endpoint.Contract.ContractBehaviors.Contains(FaultContractBehavior.Instance))
            {
                endpoint.Contract.ContractBehaviors.Add(FaultContractBehavior.Instance);
            }
        }

#if NET45
        private class SerializeExceptionErrorHandler : System.ServiceModel.Dispatcher.IErrorHandler
        {
            private readonly Func<Exception, string> _logDelegate;
            private readonly bool _includeDetails;

            public SerializeExceptionErrorHandler(Func<Exception, string> logDelegate, bool includeDetails)
            {
                _logDelegate = logDelegate;
                _includeDetails = includeDetails;
            }

            public bool HandleError(Exception error)
            {
                return !(error is System.ServiceModel.FaultException);
            }

            public void ProvideFault(Exception error, System.ServiceModel.Channels.MessageVersion version, ref System.ServiceModel.Channels.Message fault)
            {
                if (error is System.ServiceModel.FaultException)
                {
                    return;
                }

                var identifier = _logDelegate?.Invoke(error);

                var detail = new ExceptionFault(identifier, error, _includeDetails);

                var messageFault = System.ServiceModel.Channels.MessageFault.CreateFault(new System.ServiceModel.FaultCode("Sender"), new System.ServiceModel.FaultReason("UnhandledException"), detail);

                fault = System.ServiceModel.Channels.Message.CreateMessage(version, messageFault, null);
            }
        }
#endif
    }
}