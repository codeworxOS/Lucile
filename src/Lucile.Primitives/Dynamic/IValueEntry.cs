using System.Collections.Generic;

namespace Lucile.Dynamic
{
    public interface IValueEntry<out TKey, out TTarget>
    {
        TKey Key { get; }

        IEnumerable<TTarget> Targets { get; }
    }
}