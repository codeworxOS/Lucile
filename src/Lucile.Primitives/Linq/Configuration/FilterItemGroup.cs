using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using Linq.Configuration;

namespace Lucile.Linq.Configuration
{
    public class FilterItemGroup : FilterItem
    {
        public FilterItemGroup(IEnumerable<FilterItem> children, GroupType type = GroupType.And)
        {
            Children = ImmutableArray.Create(children.ToArray());
            GroupType = type;
        }

        public IReadOnlyCollection<FilterItem> Children { get; }

        public GroupType GroupType { get; }

        protected override Expression BuildExpression(ParameterExpression parameter)
        {
            Expression result = null;
            foreach (var item in Children)
            {
                var itemExpression = item.GetExpression(parameter);
                if (itemExpression != null)
                {
                    if (result == null)
                    {
                        result = itemExpression;
                    }
                    else
                    {
                        switch (GroupType)
                        {
                            case GroupType.And:
                                result = Expression.AndAlso(result, itemExpression);
                                break;

                            case GroupType.Or:
                                result = Expression.OrElse(result, itemExpression);
                                break;

                            default:
                                throw new NotImplementedException($"The GroupType {GroupType} is not implemented!");
                        }
                    }
                }
            }

            return result;
        }
    }
}