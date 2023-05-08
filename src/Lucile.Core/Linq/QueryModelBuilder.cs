using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Lucile.Data.Metadata;
using Lucile.Data.Metadata.Builder;
using Lucile.Linq.Configuration;
using Lucile.Linq.Expressions;
using Lucile.Mapper;

namespace Lucile.Linq
{
    public class QueryModelBuilder<TSource, TResult>
        where TSource : class
        where TResult : class
    {
        private readonly ConcurrentDictionary<PropertyInfo, PropertyConfigurationBuilder> _propertyBuilders;
        private readonly ConcurrentDictionary<object, SourceEntityConfigurationBuilder> _sourceBuilders;
        private Expression<Func<TSource, TResult>> _mapExpression;

        public QueryModelBuilder(Expression<Func<QuerySourceBuilder, TSource>> sourceSelector)
        {
            _sourceBuilders = new ConcurrentDictionary<object, SourceEntityConfigurationBuilder>();
            _propertyBuilders = new ConcurrentDictionary<PropertyInfo, PropertyConfigurationBuilder>();

            var methodCallExpression = sourceSelector.Body as MethodCallExpression;
            var newExpression = sourceSelector.Body as NewExpression;
            var memberInitExpression = sourceSelector.Body as MemberInitExpression;
            var param = sourceSelector.Parameters.First();
            if (IsQuerySourceBuilderGetMethodCall(param, methodCallExpression))
            {
                Source();
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

        public QueryModel<TResult> Build(IMapperFactory mapperFactory = null)
        {
            if (_mapExpression != null)
            {
                if (mapperFactory == null)
                {
                    throw new NotSupportedException("Query expression contains mapping expression, but no mapper factory was provided.");
                }

                var replaced = (Expression<Func<TSource, TResult>>)ReplaceMapCalls(mapperFactory, _mapExpression);
                Map(replaced);
            }

            List<PropertyConfiguration> propConfigs;
            List<SourceEntityConfiguration> sourceEntityConfigs;
            Data.Metadata.EntityMetadata entity;
            BuildModel(out propConfigs, out sourceEntityConfigs, out entity, mapperFactory);

            return new QueryModel<TResult>(typeof(TSource), entity, sourceEntityConfigs, propConfigs);
        }

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
            var methodCall = mapExpression.Body as MethodCallExpression;

            Dictionary<PropertyInfo, Expression> propertyMapping = null;

            if (init != null)
            {
                propertyMapping = init.Members.Select((p, i) => new { Index = i, Property = (PropertyInfo)p }).ToDictionary(p => p.Property, p => init.Arguments[p.Index]);
            }
            else if (memberInit != null)
            {
                propertyMapping = memberInit.Bindings.OfType<MemberAssignment>().ToDictionary(p => (PropertyInfo)p.Member, p => p.Expression);
            }
            else if (methodCall != null && methodCall.Method.Name == nameof(MappingExtensions.Map) && methodCall.Method.DeclaringType == typeof(MappingExtensions))
            {
                _mapExpression = mapExpression;
                return this;
            }
            else
            {
                throw new ArgumentException(nameof(mapExpression));
            }

            _mapExpression = null;

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

            if (_sourceBuilders.ContainsKey(QueryModel.GlobalSourceKey))
            {
                throw new InvalidOperationException("The is already a global QuerySource registerd.");
            }

            return (SourceEntityConfigurationBuilder<TSource, TEntity>)_sourceBuilders.GetOrAdd(propExpression.Member.Name, p => new SourceEntityConfigurationBuilder<TSource, TEntity>(p));
        }

        public SourceEntityConfigurationBuilder<TSource, TSource> Source()
        {
            if (_sourceBuilders.Keys.Except(new[] { QueryModel.GlobalSourceKey }).Any())
            {
                throw new InvalidOperationException("The is already a QuerySource for aliased entities registerd.");
            }

            return (SourceEntityConfigurationBuilder<TSource, TSource>)_sourceBuilders.GetOrAdd(QueryModel.GlobalSourceKey, p => new SourceEntityConfigurationBuilder<TSource, TSource>(p));
        }

        public SourceEntityConfigurationBuilder Source(string entityAlias)
        {
            var prop = typeof(TSource).GetProperty(entityAlias);
            if (prop == null)
            {
                throw new ArgumentException($"The source type {typeof(TSource)} does not have a property with name {entityAlias}.", nameof(entityAlias));
            }

            if (_sourceBuilders.ContainsKey(QueryModel.GlobalSourceKey))
            {
                throw new InvalidOperationException("The is already a global QuerySource registerd.");
            }

            return _sourceBuilders.GetOrAdd(entityAlias, p => (SourceEntityConfigurationBuilder)Activator.CreateInstance(typeof(SourceEntityConfigurationBuilder<,>).MakeGenericType(typeof(TSource), prop.PropertyType), p));
        }

        [Obsolete("Use Build Method instead.")]
        public QueryModel<TSource, TResult> ToModel()
        {
            List<PropertyConfiguration> propConfigs;
            List<SourceEntityConfiguration> sourceEntityConfigs;
            Data.Metadata.EntityMetadata entity;
            BuildModel(out propConfigs, out sourceEntityConfigs, out entity);

            return new QueryModel<TSource, TResult>(entity, sourceEntityConfigs, propConfigs);
        }

        private static bool IsQuerySourceBuilderGetMethodCall(ParameterExpression param, MethodCallExpression methodCallExpression)
        {
            return methodCallExpression != null && methodCallExpression.Object == param && methodCallExpression.Method.Name == "Get";
        }

        private static LambdaExpression ReplaceMapCalls(IMapperFactory mapperFactory, LambdaExpression expression)
        {
            var mapVisitor = new FindMapExpressionVisitor();
            mapVisitor.Visit(expression);
            if (mapVisitor.MapCalls.Any())
            {
                foreach (var map in mapVisitor.MapCalls)
                {
                    var arguments = map.Method.GetGenericArguments();
                    var sourceType = arguments[0];
                    var targetType = arguments[1];

                    var subMapper = mapperFactory.CreateMapper(sourceType, targetType);
                    if (map.Method.ReturnType == targetType)
                    {
                        var subLambda = subMapper.Configuration.ConversionExpression;
                        var subExpression = subLambda.Body;
                        subExpression = subExpression.Replace(subLambda.Parameters[0], map.Arguments[0]);

                        expression = expression.Replace(map, subExpression);
                    }
                    else if (map.Method.ReturnType == typeof(IEnumerable<>).MakeGenericType(targetType))
                    {
                        var selectMethod = DefaultMapperFactory.MethodInfoCache.EnumerableSelectMethod
                                                .MakeGenericMethod(sourceType, targetType);

                        expression = expression.Replace(map, Expression.Call(selectMethod, map.Arguments[0], subMapper.Configuration.ConversionExpression));
                    }
                }
            }

            return expression;
        }

        private void BuildModel(out List<PropertyConfiguration> propConfigs, out List<SourceEntityConfiguration> sourceEntityConfigs, out Data.Metadata.EntityMetadata entity, IMapperFactory mapperFactory = null)
        {
            var entityModelBuilder = new MetadataModelBuilder();
            var entityBuilder = entityModelBuilder.Entity<TResult>();

            propConfigs = new List<PropertyConfiguration>();
            sourceEntityConfigs = _sourceBuilders.Select(p => p.Value.ToTarget()).ToList();

            var props = new List<PropertySetup>();

            foreach (var item in _propertyBuilders)
            {
                var expression = item.Value.MappedExpression;

                if (mapperFactory != null)
                {
                    expression = ReplaceMapCalls(mapperFactory, expression);
                }

                props.Add(Process(expression, item.Value, entityBuilder, entityModelBuilder));

                // TODO: Merge Metadata from mapped Properties
            }

            var model = entityModelBuilder.ToModel();
            entity = model.GetEntityMetadata<TResult>();

            propConfigs.AddRange(FlattenProperties(model, props));
        }

        private IEnumerable<PropertyConfiguration> FlattenProperties(MetadataModel model, List<PropertySetup> props, PropertyConfiguration parent = null)
        {
            foreach (var item in props)
            {
                var entity = model.GetEntityMetadata(item.EntityType);
                var prop = entity[item.PropertyBuilder.PropertyName];

                var current = new PropertyConfiguration(
                                    prop,
                                    item.PropertyBuilder.Label,
                                    item.PropertyBuilder.CanAggregate,
                                    item.PropertyBuilder.CanFilter,
                                    item.PropertyBuilder.CanSort,
                                    item.PropertyBuilder.CustomFilterExpression,
                                    item.Expression,
                                    parent,
                                    !IsSingleSourceQuery);

                yield return current;

                foreach (var child in FlattenProperties(model, item.Children, current))
                {
                    yield return child;
                }
            }
        }

        private PropertySetup Process(LambdaExpression expression, PropertyConfigurationBuilder value, EntityMetadataBuilder entityBuilder, MetadataModelBuilder entityModelBuilder)
        {
            var result = new PropertySetup
            {
                EntityType = entityBuilder.TypeInfo.ClrType,
                Expression = expression,
                PropertyBuilder = value
            };

            if (expression.Body.NodeType == ExpressionType.New || expression.Body.NodeType == ExpressionType.MemberInit)
            {
                var nav = entityBuilder.Navigation(value.PropertyName);
                var targetEntity = entityModelBuilder.Entity(nav.Target.ClrType);
                result.Children.AddRange(expression.GetPropertyLambda().Select(p => Process(p.Value, value.Property(p.Key), targetEntity, entityModelBuilder)));
            }
            else
            {
                entityBuilder.Property(value.PropertyName, value.PropertyType);
                if (value.IsPrimaryKey)
                {
                    entityBuilder.PrimaryKey.Add(value.PropertyName);
                }
            }

            return result;
        }

        private class PropertySetup
        {
            public PropertySetup()
            {
                this.Children = new List<PropertySetup>();
            }

            public List<PropertySetup> Children { get; }

            public Type EntityType { get; set; }

            public LambdaExpression Expression { get; set; }

            public PropertyConfigurationBuilder PropertyBuilder { get; set; }
        }
    }
}