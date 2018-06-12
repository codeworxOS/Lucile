using System.ServiceModel;
using System.Threading.Tasks;

namespace Tests
{
    [ServiceContract]
    public interface IMessageScopeService
    {
        [OperationContract]
        Task<string> TestAsync();
    }
}