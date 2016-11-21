using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using Linq.Configuration;

namespace Lucile.Linq.Configuration.Builder
{
    [DataContract(IsReference = true)]
    public class FilterItemGroupBuilder : FilterItemBuilder
    {
        public FilterItemGroupBuilder()
        {
            this.Children = new Collection<FilterItemBuilder>();
        }

        [DataMember(Order = 1)]
        public Collection<FilterItemBuilder> Children { get; }

        [DataMember(Order = 2)]
        public GroupType GroupType { get; set; }

        public override void LoadFrom(FilterItem value)
        {
            var group = value as FilterItemGroup;
            if (group != null)
            {
                foreach (var item in group.Children)
                {
                    Children.Add(FilterItemBuilder.GetBuilder(item));
                }

                GroupType = group.GroupType;
            }
            else
            {
                throw new ArgumentException($"Unexpected FilterItem. Expected {nameof(FilterItemGroup)} got {value?.GetType()}", nameof(value));
            }
        }

        public override FilterItem ToTarget()
        {
            return new FilterItemGroup(Children.Select(p => p.ToTarget()), GroupType);
        }
    }
}