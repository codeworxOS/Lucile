using System.ServiceModel;
using System.Threading.Tasks;

namespace Lucile.Core.Test.Module1
{
    [ServiceContract]
    public interface ITestService
    {
        Task<TestData> GetTestDataAsync(int id);
    }
}