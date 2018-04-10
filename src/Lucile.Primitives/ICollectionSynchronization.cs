using System.Collections;

namespace Lucile
{
    public interface ICollectionSynchronization
    {
        void DisableCollectionSynchronization(IEnumerable collection);

        void EnableCollectionSynchronization(IEnumerable collection, object lockObject);
    }
}