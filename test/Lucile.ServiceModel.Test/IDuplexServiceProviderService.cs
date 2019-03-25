using System.ServiceModel;
using System.Threading.Tasks;

namespace Lucile.ServiceModel.Test
{
    [ServiceContract(CallbackContract = typeof(IDuplexCallback))]
    public interface IDuplexServiceProviderService
    {
        [OperationContract]
        Task StartImportAsync();
    }
}