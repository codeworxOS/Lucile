using System.Collections;

namespace Lucile
{
    public interface ICollectionSynchronization
    {
        void Disable(IEnumerable collection);

        void Enable(IEnumerable collection, object lockObject);
    }
}