using System.Collections;

namespace Codeworx
{
    public interface ICollectionSynchronization
    {
        void EnableCollectionSynchronization(IEnumerable collection, object lockObject);

        void DisableCollectionSynchronization(IEnumerable collection);
    }
}
