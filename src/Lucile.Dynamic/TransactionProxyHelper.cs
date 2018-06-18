using System.Collections.Generic;
using System.Linq;

namespace Lucile.Dynamic
{
    public class TransactionProxyHelper
    {
        public static void AddValue<TValue, TTarget>(TValue value, TTarget target, Dictionary<Key<TValue>, List<TTarget>> dictionary)
        {
            if (!dictionary.ContainsKey(value))
            {
                dictionary.Add(value, new List<TTarget>());
            }

            dictionary[value].Add(target);
        }

        public static TValue GetValue<TValue, TTarget>(Dictionary<Key<TValue>, List<TTarget>> dictionary)
        {
            if (dictionary.Count == 1)
            {
                return dictionary.Keys.First().Value;
            }

            return default(TValue);
        }

        public static void SetCollectionValue<TValue, TCollection, TTarget>(Dictionary<Key<TCollection>, List<TTarget>> dictionary, ICollection<TValue> targetCollection)
            where TCollection : IEnumerable<TValue>
        {
            foreach (var item in dictionary.Keys.Where(p => p.HasValue).SelectMany(p => p.Value))
            {
                targetCollection.Add(item);
            }
        }
    }
}