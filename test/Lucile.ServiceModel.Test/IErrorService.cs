using System.ServiceModel;
using System.ServiceModel.Web;
using System.Threading.Tasks;

namespace Lucile.ServiceModel.Test
{
    [ServiceContract]
    public interface IErrorService
    {
        [OperationContract]
        [WebGet(UriTemplate = "RaiseAsync")]
        Task RaiseAsyncErrorAsync();

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "Raise")]
        void RaiseError();
    }
}