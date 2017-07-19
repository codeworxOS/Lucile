using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Lucile.Data.Metadata;

namespace Lucile.Data
{
    public class AttachOperations<T>
    {
        private readonly ConcurrentDictionary<object, IEnumerable<ScalarProperty>> _mergedItems;
        private readonly List<object> _newlyAdded;

        public AttachOperations(IEnumerable<T> items)
        {
            Items = items;
            _newlyAdded = new List<object>();
            Added = new ReadOnlyCollection<object>(_newlyAdded);
            _mergedItems = new ConcurrentDictionary<object, IEnumerable<ScalarProperty>>();
            Merged = new ReadOnlyDictionary<object, IEnumerable<ScalarProperty>>(_mergedItems);
        }

        public IReadOnlyCollection<object> Added { get; }

        public IEnumerable<T> Items { get; }

        public IReadOnlyDictionary<object, IEnumerable<ScalarProperty>> Merged { get; }

        public void TrackAdded(object item)
        {
            _newlyAdded.Add(item);
        }

        public void TrackMerged(object item, IEnumerable<ScalarProperty> properties)
        {
            _mergedItems.AddOrUpdate(item, properties, (k, i) => i.Union(properties).ToList());
        }
    }
}