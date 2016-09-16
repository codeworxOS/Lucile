using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Lucile.Linq.Configuration;

namespace Lucile.Linq
{
    public class QueryConfiguration
    {
        public QueryConfiguration(IEnumerable<SelectItem> selectItems, IEnumerable<SortItem> sortItems)
        {
            Select = ImmutableArray.Create<SelectItem>(selectItems.ToArray());
            Sort = ImmutableArray.Create<SortItem>(sortItems.ToArray());
        }

        public IReadOnlyCollection<SelectItem> Select { get; }

        public IReadOnlyCollection<SortItem> Sort { get; set; }
    }
}
