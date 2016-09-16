using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Codeworx.ComponentModel;

namespace Codeworx
{
    public class CollectionSynchronization
    {
        public static void Register(IEnumerable collection, object lockObject)
        {
            var items = CompositionContext.Current.GetExports<ICollectionSynchronization>();
            foreach (var item in items)
            {
                item.EnableCollectionSynchronization(collection, lockObject);
            }
        }

        public static void Unregister(IEnumerable collection)
        {
            var items = CompositionContext.Current.GetExports<ICollectionSynchronization>();
            foreach (var item in items)
            {
                item.DisableCollectionSynchronization(collection);
            }
        }
    }
}
