using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Lucile.Data.Metadata;

namespace Lucile.Linq.Configuration
{
    public class PropertyConfiguration
    {
        public PropertyConfiguration(PropertyMetadata property, Func<string> label, bool canAggregate, bool canFilter, bool canSort, Func<object, LambdaExpression> customFilterExpression, LambdaExpression mappedExpression, PropertyConfiguration parent = null, bool calculcateDependencies = true)
        {
            Property = property;
            Parent = parent;

            // TODO replace with propertyInfo from PropertyMetadata object
            PropertyInfo = property.Entity.ClrType.GetProperty(property.Name);
            Label = label;
            CanAggregate = canAggregate;
            CanFilter = canFilter;
            CanSort = canSort;
            CustomFilterExpression = customFilterExpression;
            MappedExpression = mappedExpression;

            List<string> members = new List<string>();

            if (MappedExpression != null && calculcateDependencies)
            {
                var param = MappedExpression.Parameters.First();
                members.AddRange(MappedExpression.Find<MemberExpression>(p => p.Expression == param).Select(p => p.Member.Name));
            }

            DependsOn = ImmutableArray<string>.Empty.AddRange(members);
        }

        public bool CanAggregate { get; }

        public bool CanFilter { get; }

        public bool CanSort { get; }

        public Func<object, LambdaExpression> CustomFilterExpression { get; }

        public IReadOnlyCollection<string> DependsOn { get; }

        public Func<string> Label { get; }

        public LambdaExpression MappedExpression { get; }

        public PropertyMetadata Property { get; }

        public PropertyConfiguration Parent { get; }

        public PropertyInfo PropertyInfo { get; }

        public string PropertyPath => Parent != null ? $"{Parent.PropertyPath}.{Property.Name}" : Property.Name;
    }
}