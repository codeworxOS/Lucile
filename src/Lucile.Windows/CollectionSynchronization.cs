using System.Collections;
using System.Windows.Data;

namespace Lucile.Windows
{
    public class CollectionSynchronization : ICollectionSynchronization
    {
        public void Disable(IEnumerable collection)
        {
            BindingOperations.DisableCollectionSynchronization(collection);
        }

        public void Enable(IEnumerable collection, object lockObject)
        {
            BindingOperations.EnableCollectionSynchronization(collection, lockObject);
        }
    }
}