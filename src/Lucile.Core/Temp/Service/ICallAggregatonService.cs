using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Codeworx.Service
{
    [ServiceContract]
    [ServiceKnownType("GetKnownTypes",typeof(KnownTypeProvider))]
    public interface ICallAggregatonService
    {
        [OperationContract]
        Task<IEnumerable<CallAggregationResult>> GetResultsAsync(IEnumerable<CallAggregationServiceDescription> services);
    }
}
