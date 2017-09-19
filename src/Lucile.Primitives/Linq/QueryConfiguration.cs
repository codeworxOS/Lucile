using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Lucile.Linq.Configuration;

namespace Lucile.Linq
{
    public class QueryConfiguration
    {
        public QueryConfiguration()
            : this(Enumerable.Empty<SelectItem>(), Enumerable.Empty<SortItem>(), Enumerable.Empty<FilterItem>())
        {
        }

        public QueryConfiguration(IEnumerable<FilterItem> filterItems)
            : this(Enumerable.Empty<SelectItem>(), Enumerable.Empty<SortItem>(), filterItems)
        {
        }

        public QueryConfiguration(IEnumerable<SelectItem> selectItems)
            : this(selectItems, Enumerable.Empty<SortItem>(), Enumerable.Empty<FilterItem>())
        {
        }

        public QueryConfiguration(IEnumerable<SortItem> sortItems, IEnumerable<FilterItem> filterItems)
            : this(Enumerable.Empty<SelectItem>(), sortItems, filterItems)
        {
        }

        public QueryConfiguration(IEnumerable<SelectItem> selectItems, IEnumerable<SortItem> sortItems, IEnumerable<FilterItem> filterItems)
            : this(selectItems, sortItems, filterItems, Enumerable.Empty<FilterItem>())
        {
        }

        public QueryConfiguration(IEnumerable<SelectItem> selectItems, IEnumerable<SortItem> sortItems, IEnumerable<FilterItem> filterItems, IEnumerable<FilterItem> targetFilterItems)
        {
            Select = ImmutableArray.Create<SelectItem>(selectItems.ToArray());
            Sort = ImmutableArray.Create<SortItem>(sortItems.ToArray());
            FilterItems = ImmutableArray.Create<FilterItem>(filterItems.ToArray());
            TargetFilterItems = ImmutableArray.Create<FilterItem>(targetFilterItems.ToArray());
        }

        public IReadOnlyCollection<FilterItem> FilterItems { get; }

        public IReadOnlyCollection<SelectItem> Select { get; }

        public IReadOnlyCollection<SortItem> Sort { get; }

        public IReadOnlyCollection<FilterItem> TargetFilterItems { get; }
    }
}