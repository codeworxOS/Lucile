using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace Lucile.Linq.Configuration
{
    public class PropertyConfigurationBuilder<TSource, TValue> : PropertyConfigurationBuilder
    {
        private readonly ConcurrentDictionary<PropertyInfo, PropertyConfigurationBuilder> _children = new ConcurrentDictionary<PropertyInfo, PropertyConfigurationBuilder>();

        private LambdaExpression _mappedExpression;

        public PropertyConfigurationBuilder(PropertyInfo propertyInfo)
            : base(propertyInfo)
        {
        }

        public PropertyConfigurationBuilder(Expression<Func<TSource, TValue>> propSelector)
            : this(propSelector.GetPropertyInfo())
        {
        }

        public override LambdaExpression MappedExpression
        {
            get
            {
                return _mappedExpression;
            }

            set
            {
                _mappedExpression = value;
                _children.Clear();
                foreach (var item in _mappedExpression.GetPropertyLambda())
                {
                    var builder = Property(item.Key);
                    builder.MappedExpression = item.Value;
                }
            }
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

        public override PropertyConfigurationBuilder Property(PropertyInfo child)
        {
            return _children.GetOrAdd(child, p => (PropertyConfigurationBuilder)Activator.CreateInstance(typeof(PropertyConfigurationBuilder<,>).MakeGenericType(PropertyType, p.PropertyType), p));
        }

        public PropertyConfigurationBuilder<TValue, TChildValue> Property<TChildValue>(Expression<Func<TValue, TChildValue>> selector)
        {
            if (selector.Body is MemberExpression member && member.Member is PropertyInfo property)
            {
                return (PropertyConfigurationBuilder<TValue, TChildValue>)_children.GetOrAdd(property, p => new PropertyConfigurationBuilder<TValue, TChildValue>(p));
            }

            throw new ArgumentOutOfRangeException(nameof(selector), "Selector must be a property expression.");
        }

        public PropertyConfigurationBuilder<TSource, TValue> Sortable(bool value = true)
        {
            CanSort = value;
            return this;
        }
    }
}