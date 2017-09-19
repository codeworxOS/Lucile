﻿using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using Lucile.Builder;
using Lucile.Linq.Configuration;
using Lucile.Linq.Configuration.Builder;

namespace Lucile.Linq.Builder
{
    [DataContract(IsReference = true)]
    public class QueryConfigurationBuilder : BaseBuilder<QueryConfiguration>
    {
        public QueryConfigurationBuilder()
        {
            this.FilterItems = new Collection<FilterItemBuilder>();
            this.TargetFilterItems = new Collection<FilterItemBuilder>();
            this.SortItems = new Collection<SortItemBuilder>();
            this.Select = new Collection<SelectItem>();
        }

        [DataMember(Order = 1)]
        public Collection<FilterItemBuilder> FilterItems { get; }

        [DataMember(Order = 3)]
        public Collection<SelectItem> Select { get; }

        [DataMember(Order = 2)]
        public Collection<SortItemBuilder> SortItems { get; }

        [DataMember(Order = 4)]
        public Collection<FilterItemBuilder> TargetFilterItems { get; }

        public override void LoadFrom(QueryConfiguration value)
        {
            foreach (var item in value.FilterItems.Select(p => FilterItemBuilder.GetBuilder(p)))
            {
                this.FilterItems.Add(item);
            }

            foreach (var item in value.Select)
            {
                this.Select.Add(item);
            }

            foreach (var item in value.Sort.Select(p =>
                {
                    var b = new SortItemBuilder();
                    b.LoadFrom(p);
                    return b;
                }))
            {
                this.SortItems.Add(item);
            }
        }

        public override QueryConfiguration ToTarget()
        {
            return new QueryConfiguration(this.Select, this.SortItems.Select(p => p.ToTarget()), this.FilterItems.Select(p => p.ToTarget()), this.TargetFilterItems.Select(p => p.ToTarget()));
        }
    }
}