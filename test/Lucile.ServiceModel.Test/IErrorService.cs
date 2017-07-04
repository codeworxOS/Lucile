using System.ServiceModel;
using System.Threading.Tasks;

namespace Lucile.ServiceModel.Test
{
    [ServiceContract]
    public interface IErrorService
    {
        [OperationContract]
        Task RaiseAsyncErrorAsync();

        [OperationContract]
        void RaiseError();
    }
}