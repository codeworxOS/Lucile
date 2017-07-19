using System;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace Lucile.ServiceModel.Behavior
{
    public class UnWrapExceptionFromFaultInspector : IClientMessageInspector
    {
        public void AfterReceiveReply(ref System.ServiceModel.Channels.Message reply, object correlationState)
        {
            if (reply.IsFault || IsHttpInternalErrorFault(reply))
            {
                ExceptionFault fault = null;
                try
                {
                    var mf = MessageFault.CreateFault(reply, int.MaxValue);
                    if (mf.HasDetail && mf.Reason.ToString() == "UnhandledException")
                    {
                        var detail = mf.GetDetail<ExceptionFault>();
                        fault = detail;
                    }
                }
                catch
                {
                    // do nothing
                }

                fault.ReThrow();
            }
        }

        public object BeforeSendRequest(ref System.ServiceModel.Channels.Message request, System.ServiceModel.IClientChannel channel)
        {
            return null;
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