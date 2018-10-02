using System;
using System.Collections.Generic;
using System.Text;

namespace Tests
{
    public static class UnitTestCollectionExtensions
    {
        public static void AddRange<TElement>(this ICollection<TElement> collection, IEnumerable<TElement> items)
        {
            foreach (var item in items)
            {
                collection.Add(item);
            }
        }
    }
}