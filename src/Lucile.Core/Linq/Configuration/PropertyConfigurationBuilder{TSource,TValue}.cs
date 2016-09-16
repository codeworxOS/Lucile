using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Lucile.Linq.Configuration
{
    public class PropertyConfigurationBuilder<TSource, TValue> : PropertyConfigurationBuilder
    {
        public PropertyConfigurationBuilder(PropertyInfo propertyInfo)
            : base(propertyInfo)
        {
        }

        public PropertyConfigurationBuilder(Expression<Func<TSource, TValue>> propSelector)
            : this(propSelector.GetPropertyInfo())
        {
        }

        public PropertyConfigurationBuilder<TSource, TValue> Aggregateable(bool value = true)
        {
            CanAggregate = value;
            return this;
        }

        public PropertyConfigurationBuilder<TSource, TValue> Filterable(bool value = true)
        {
            CanFilter = value;
            return this;
        }

        public PropertyConfigurationBuilder<TSource, TValue> FilterExpression(Func<TValue, Expression<Func<TSource, bool>>> filterFactory)
        {
            CustomFilterExpression = p => filterFactory((TValue)p);

            return this;
        }

        public PropertyConfigurationBuilder<TSource, TValue> HasLabel(string label)
        {
            Label = () => label;
            return this;
        }

        public PropertyConfigurationBuilder<TSource, TValue> HasLabel(Func<string> label)
        {
            Label = label;
            return this;
        }

        public PropertyConfigurationBuilder<TSource, TValue> Map(Expression<Func<TSource, TValue>> valueExpression)
        {
            this.MappedExpression = valueExpression;
            return this;
        }

        public PropertyConfigurationBuilder<TSource, TValue> Sortable(bool value = true)
        {
            CanSort = value;
            return this;
        }
    }
}