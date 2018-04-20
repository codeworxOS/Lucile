using System.Linq;
using System.Runtime.ExceptionServices;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace Lucile.ServiceModel.Behavior
{
    public class UnWrapExceptionFromFaultInspector : IClientMessageInspector
    {
        private readonly ContractDescription _contract;
        private readonly ClientRuntime _runtime;

        public UnWrapExceptionFromFaultInspector(ClientRuntime runtime, ContractDescription contract)
        {
            _contract = contract;
            _runtime = runtime;
        }

        public void AfterReceiveReply(ref System.ServiceModel.Channels.Message reply, object correlationState)
        {
            if (reply.IsFault || IsHttpInternalErrorFault(reply))
            {
                ExceptionFault fault = null;
                var mf = MessageFault.CreateFault(reply, int.MaxValue);
                try
                {
                    if (mf.HasDetail && mf.Reason.ToString() == "UnhandledException")
                    {
                        var detail = mf.GetDetail<ExceptionFault>();
                        fault = detail;
                    }
                }
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                catch
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
                {
                    // do nothing
                }

                if (fault != null)
                {
                    fault.ReThrow();
                }
                else
                {
                    var operation = _runtime.ClientOperations.FirstOrDefault(p => p.Action == (string)correlationState);
                    var faultTypes = _contract.Operations.Where(p => operation == null || p.Name == operation.Name).SelectMany(p => p.Faults.Select(x => x.DetailType)).Distinct().ToArray();

                    var ex = FaultException.CreateFault(mf, reply.Headers.Action, faultTypes);
                    ExceptionDispatchInfo.Capture(ex).Throw();
                }
            }
        }

        public object BeforeSendRequest(ref System.ServiceModel.Channels.Message request, System.ServiceModel.IClientChannel channel)
        {
            return request.Headers.Action;
        }

        private bool IsHttpInternalErrorFault(Message reply)
        {
            if (reply.Properties.ContainsKey(HttpResponseMessageProperty.Name))
            {
                var response = reply.Properties[HttpResponseMessageProperty.Name] as HttpResponseMessageProperty;
                if (response != null)
                {
                    return response.StatusCode == System.Net.HttpStatusCode.InternalServerError;
                }
            }

            return false;
        }
    }
}