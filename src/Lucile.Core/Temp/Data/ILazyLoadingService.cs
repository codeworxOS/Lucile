using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Codeworx.Data.Metadata;

namespace Codeworx.Data
{
    [ServiceContract]
    public interface ILazyLoadingService
    {
        [OperationContract]
        Task<IEnumerable<NavigationPropertyValues>> GetPropertyValuesAsync(IEnumerable<EntityKey> keys, IncludePaths includes);
    }
}
