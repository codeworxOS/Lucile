using System.Collections.Generic;

namespace Lucile.Dynamic
{
    public class ValueEntry<TKey, TTarget> : IValueEntry<TKey, TTarget>
    {
        public ValueEntry(TKey key, IEnumerable<TTarget> targets)
        {
            Key = key;
            Targets = targets;
        }

        public TKey Key { get; }

        public IEnumerable<TTarget> Targets { get; }
    }
}