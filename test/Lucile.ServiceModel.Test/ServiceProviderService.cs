using System.ServiceModel;
using System.Threading.Tasks;

namespace Lucile.ServiceModel.Test
{
    public class ServiceProviderService : IServiceProviderService
    {
        public async Task<string> GetTestHeaderContent()
        {
            return OperationContext.Current.IncomingMessageHeaders.GetHeader<string>("TestHeader", "TestNamespace");
        }

        public async Task<string> Hello(string message)
        {
            return message;
        }
    }
}