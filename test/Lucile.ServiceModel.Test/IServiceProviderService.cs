using System.ServiceModel;
using System.Threading.Tasks;

namespace Lucile.ServiceModel.Test
{
    [ServiceContract]
    public interface IServiceProviderService
    {
        [OperationContract]
        Task<string> GetTestHeaderContent();

        [OperationContract]
        Task<string> Hello(string message);
    }
}