using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Lucile.Data.Metadata.Builder;
using Lucile.Linq.Configuration;

namespace Lucile.Linq
{
    public class QueryModelBuilder<TSource, TResult>
        where TSource : class
        where TResult : class
    {
        private readonly ConcurrentDictionary<PropertyInfo, PropertyConfigurationBuilder> _propertyBuilders;
        private readonly ConcurrentDictionary<string, SourceEntityConfigurationBuilder> _sourceBuilders;

        public QueryModelBuilder(Expression<Func<QuerySourceBuilder, TSource>> sourceSelector)
        {
            _sourceBuilders = new ConcurrentDictionary<string, SourceEntityConfigurationBuilder>();
            _propertyBuilders = new ConcurrentDictionary<PropertyInfo, PropertyConfigurationBuilder>();

            var methodCallExpression = sourceSelector.Body as MethodCallExpression;
            var newExpression = sourceSelector.Body as NewExpression;
            var memberInitExpression = sourceSelector.Body as MemberInitExpression;
            var param = sourceSelector.Parameters.First();
            if (IsQuerySourceBuilderGetMethodCall(param, methodCallExpression))
            {
                IsSingleSourceQuery = true;
            }
            else if (newExpression != null && newExpression.Arguments.All(p => IsQuerySourceBuilderGetMethodCall(param, p as MethodCallExpression)))
            {
                foreach (var item in newExpression.Members)
                {
                    var source = Source(item.Name);
                }
            }
            else if (memberInitExpression != null && memberInitExpression.Bindings.Cast<MemberAssignment>().All(p => IsQuerySourceBuilderGetMethodCall(param, p.Expression as MethodCallExpression)))
            {
                foreach (var item in memberInitExpression.Bindings)
                {
                    var source = Source(item.Member.Name);
                }
            }
            else
            {
                throw new ArgumentException("Only direct calls to the p.Get<T>() method of the QuerySourceBuilder parameter or Member initializations (new { Prop = p.Get<T>()}) with calls to the Get method are allowed.", nameof(sourceSelector));
            }
        }

        public bool IsSingleSourceQuery { get; }

        public QueryModelBuilder<TSource, TResult> HasKey<TKey>(Expression<Func<TResult, TKey>> keySelector)
        {
            var member = keySelector.Body as MemberExpression;
            var newExpression = keySelector.Body as NewExpression;
            if (member != null && member.Expression == keySelector.Parameters.First() && member.Member is PropertyInfo)
            {
                var prop = Property((PropertyInfo)member.Member);
                prop.IsPrimaryKey = true;
            }
            else if (newExpression != null && newExpression.Arguments.All(p => p is MemberExpression && ((MemberExpression)p).Member is PropertyInfo && ((MemberExpression)p).Expression == keySelector.Parameters.First()))
            {
                foreach (var item in newExpression.Arguments.Cast<MemberExpression>())
                {
                    var prop = Property((PropertyInfo)item.Member);
                    prop.IsPrimaryKey = true;
                }
            }
            else
            {
                throw new NotSupportedException("The provided keySelector expression must be a direct Member expression or a new expression with only direct members");
            }

            return this;
        }

        public QueryModelBuilder<TSource, TResult> Map(Expression<Func<TSource, TResult>> mapExpression)
        {
            var init = mapExpression.Body as NewExpression;
            var memberInit = mapExpression.Body as MemberInitExpression;

            Dictionary<PropertyInfo, Expression> propertyMapping = null;

            if (init != null)
            {
                propertyMapping = init.Members.Select((p, i) => new { Index = i, Property = (PropertyInfo)p }).ToDictionary(p => p.Property, p => init.Arguments[p.Index]);
            }
            else if (memberInit != null)
            {
                propertyMapping = memberInit.Bindings.OfType<MemberAssignment>().ToDictionary(p => (PropertyInfo)p.Member, p => p.Expression);
            }
            else
            {
                throw new ArgumentException(nameof(mapExpression));
            }

            foreach (var item in propertyMapping)
            {
                var prop = Property(item.Key);
                prop.MappedExpression = Expression.Lambda(item.Value, mapExpression.Parameters.First());
            }

            return this;
        }

        public PropertyConfigurationBuilder<TSource, TValue> Property<TValue>(Expression<Func<TResult, TValue>> propertySelector)
        {
            var propInfo = propertySelector.GetPropertyInfo();
            return (PropertyConfigurationBuilder<TSource, TValue>)_propertyBuilders.GetOrAdd(propInfo, p => new PropertyConfigurationBuilder<TSource, TValue>(p));
        }

        public PropertyConfigurationBuilder Property(PropertyInfo propInfo)
        {
            return (PropertyConfigurationBuilder)_propertyBuilders.GetOrAdd(propInfo, p => (PropertyConfigurationBuilder)Activator.CreateInstance(typeof(PropertyConfigurationBuilder<,>).MakeGenericType(typeof(TSource), propInfo.PropertyType), p));
        }

        public SourceEntityConfigurationBuilder<TSource, TEntity> Source<TEntity>(Expression<Func<TSource, TEntity>> entitySelector)
        {
            var propExpression = entitySelector.Body as MemberExpression;
            if (propExpression == null || propExpression.Expression.Type != typeof(TSource))
            {
                throw new ArgumentException($"The given lambda expression is in a wrong format. Only Property Expressions are allowed in the body", nameof(entitySelector));
            }

            return (SourceEntityConfigurationBuilder<TSource, TEntity>)_sourceBuilders.GetOrAdd(propExpression.Member.Name, p => new SourceEntityConfigurationBuilder<TSource, TEntity>());
        }

        public SourceEntityConfigurationBuilder Source(string entityAlias)
        {
            var prop = typeof(TSource).GetProperty(entityAlias);
            if (prop == null)
            {
                throw new ArgumentException($"The source type {typeof(TSource)} does not have a property with name {entityAlias}.", nameof(entityAlias));
            }

            return _sourceBuilders.GetOrAdd(entityAlias, p => (SourceEntityConfigurationBuilder)Activator.CreateInstance(typeof(SourceEntityConfigurationBuilder<,>).MakeGenericType(typeof(TSource), prop.PropertyType)));
        }

        public QueryModel<TSource, TResult> ToModel()
        {
            var entityModelBuilder = new MetadataModelBuilder();
            var entityBuilder = entityModelBuilder.Entity<TResult>();

            var propConfigs = new List<PropertyConfiguration>();

            var sourceEntityConfigs = new List<SourceEntityConfiguration>();

            foreach (var item in _sourceBuilders)
            {
                var entityType = typeof(TSource).GetProperty(item.Key).PropertyType;

                var sourceConfig = new SourceEntityConfiguration(item.Key, entityType, item.Value.QueryFactory, item.Value.JoinKeyType, item.Value.LocalJoinExpression, item.Value.RemoteJoinExpression);
                sourceEntityConfigs.Add(sourceConfig);
            }

            foreach (var item in _propertyBuilders)
            {
                entityBuilder.Property(item.Value.PropertyName);
                if (item.Value.IsPrimaryKey)
                {
                    entityBuilder.PrimaryKey.Add(item.Value.PropertyName);
                }

                // TODO: Merge Metadata from mapped Properties
            }

            var model = entityModelBuilder.ToModel();
            var entity = model.GetEntityMetadata<TResult>();

            foreach (var item in _propertyBuilders)
            {
                var prop = entity[item.Value.PropertyName];
                var config = new PropertyConfiguration(prop, item.Value.Label, item.Value.CanAggregate, item.Value.CanFilter, item.Value.CanSort, item.Value.CustomFilterExpression, item.Value.MappedExpression, !IsSingleSourceQuery);
                propConfigs.Add(config);
            }

            return new QueryModel<TSource, TResult>(entity, sourceEntityConfigs, propConfigs);
        }

        private static bool IsQuerySourceBuilderGetMethodCall(ParameterExpression param, MethodCallExpression methodCallExpression)
        {
            return methodCallExpression != null && methodCallExpression.Object == param && methodCallExpression.Method.Name == "Get";
        }
    }
}