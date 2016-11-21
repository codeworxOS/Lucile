using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Lucile.Data.Metadata.Builder;

namespace Lucile.Data.Metadata
{
    public class EntityMetadata : MetadataElement
    {
        private readonly Func<object, bool> _checkTypeDelegate;
        private readonly Func<object, object, bool> _keyEqualsDelegate;

        internal EntityMetadata(ModelCreationScope scope, EntityMetadataBuilder builder)
            : base(builder.Name)
        {
            scope.AddEntity(builder.TypeInfo.ClrType, this);
            this.ClrType = builder.TypeInfo.ClrType;

            var propertyListBuilder = ImmutableList.CreateBuilder<ScalarProperty>();
            var navigationListBuilder = ImmutableList.CreateBuilder<NavigationPropertyMetadata>();

            if (builder.BaseEntity != null)
            {
                BaseEntity = scope.GetEntity(builder.BaseEntity.TypeInfo.ClrType);
                foreach (var prop in BaseEntity.Properties)
                {
                    propertyListBuilder.Add(prop);
                }

                foreach (var prop in BaseEntity.Navigations)
                {
                    navigationListBuilder.Add(prop);
                }
            }

            var properties = builder.Properties.OrderBy(p => builder.PrimaryKey.Contains(p.Name) ? builder.PrimaryKey.IndexOf(p.Name) : builder.PrimaryKey.Count).ToList();

            foreach (var item in properties)
            {
                propertyListBuilder.Add(item.ToProperty(this, builder.PrimaryKey.Contains(item.Name)));
            }

            Properties = propertyListBuilder.ToImmutable();

            foreach (var item in builder.Navigations)
            {
                navigationListBuilder.Add(item.ToNavigation(scope, this));
            }

            Navigations = navigationListBuilder.ToImmutable();

            this._checkTypeDelegate = GetCheckTypeDelegate(ClrType);
            var primaryKeys = this.Properties.Where(p => p.IsPrimaryKey);
            if (primaryKeys.Any())
            {
                PrimaryKeyType = primaryKeys.First().PropertyType;
                this._keyEqualsDelegate = GetKeyEqualsDelegate(ClrType, primaryKeys);
            }
        }

        public EntityMetadata BaseEntity
        {
            get;
        }

        public ImmutableList<EntityMetadata> ChildEntities
        {
            get;
        }

        public Type ClrType
        {
            get;
        }

        public ImmutableList<NavigationPropertyMetadata> Navigations { get; }

        public Type PrimaryKeyType
        {
            get;
        }

        public ImmutableList<ScalarProperty> Properties { get; }

        public PropertyMetadata this[string propertyName]
        {
            get
            {
                return (PropertyMetadata)Properties.FirstOrDefault(p => p.Name == propertyName) ?? Navigations.FirstOrDefault(p => p.Name == propertyName);
            }
        }

        public static Expression GetPropertyCompareExpression(MemberExpression propLeft, MemberExpression propRight)
        {
            Expression expression;
            if (propLeft.Type.GetTypeInfo().IsPrimitive)
            {
                expression = Expression.Equal(propLeft, propRight);
            }
            else
            {
                var equalsMethod = propLeft.Type.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                    .Where(p => p.Name == "Equals" && p.GetParameters().Count() == 1 && (p.GetParameters().First().ParameterType == typeof(object) || p.GetParameters().First().ParameterType == propLeft.Type))
                    .OrderBy(p => p.GetParameters().First().ParameterType == propLeft.Type ? 0 : 1)
                    .FirstOrDefault();

                expression = Expression.Call(
                                    propLeft,
                                    equalsMethod,
                                    equalsMethod.GetParameters().First().ParameterType == typeof(object) ? (Expression)Expression.Convert(propRight, typeof(object)) : propRight);
            }

            return expression;
        }

        public Dictionary<object, NavigationPropertyMetadata> FlattenChildren(object entity, int maxLevel = 0)
        {
            var children = new Dictionary<object, NavigationPropertyMetadata>();
            FlattenChildren(entity, children, this, 0, maxLevel);
            return children;
        }

        public object GetPrimaryKeyObject(object entity)
        {
            if (PrimaryKeyType == null)
            {
                throw new NotImplementedException("Currently only single Primary Key Objects are supported.");
            }

            return this.Properties.First(p => p.IsPrimaryKey).GetValue(entity);
        }

        public bool IsOfType(object parameter)
        {
            return _checkTypeDelegate(parameter);
        }

        public bool IsPrimaryKeySet(object source)
        {
            foreach (var item in Properties.Where(p => p.IsPrimaryKey))
            {
                if (!object.Equals(item.GetValue(source), item.DefaultValue))
                {
                    return true;
                }
            }

            return false;
        }

        public bool KeyEquals(object left, object right)
        {
            if (_keyEqualsDelegate == null)
            {
                throw new NotSupportedException($"No Primary Key definde for Entity {Name}!");
            }

            return this._keyEqualsDelegate(left, right);
        }

        private static void FlattenChildren(object parent, IDictionary<object, NavigationPropertyMetadata> children, EntityMetadata entity, int level, int maxLevel = 0)
        {
            var concreteEntity = entity.FlattenEntities().First(p => p.IsOfType(parent));

            foreach (var item in concreteEntity.Properties.OfType<NavigationPropertyMetadata>())
            {
                var value = item.GetValue(parent);
                if (value != null)
                {
                    var enumerable = value as IEnumerable<object>;

                    if (enumerable != null)
                    {
                        foreach (var child in enumerable)
                        {
                            if (!children.ContainsKey(child))
                            {
                                children.Add(child, item);
                                if (maxLevel == 0 || level + 1 < maxLevel)
                                {
                                    FlattenChildren(child, children, item.TargetEntity, level + 1, maxLevel);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (!children.ContainsKey(value))
                        {
                            children.Add(value, item);
                            if (maxLevel == 0 || level + 1 < maxLevel)
                            {
                                FlattenChildren(value, children, item.TargetEntity, level + 1, maxLevel);
                            }
                        }
                    }
                }
            }
        }

        private static Func<object, bool> GetCheckTypeDelegate(Type clrType)
        {
            var parameterExpresssion = Expression.Parameter(typeof(object));

            var isExpression = Expression.TypeIs(parameterExpresssion, clrType);
            var lambda = Expression.Lambda<Func<object, bool>>(isExpression, parameterExpresssion);
            return lambda.Compile();
        }

        private static Func<object, object, bool> GetKeyEqualsDelegate(Type clrType, IEnumerable<ScalarProperty> keyProperties)
        {
            Expression compareExpression = null;
            ParameterExpression paramLeft = Expression.Parameter(typeof(object));
            ParameterExpression paramRight = Expression.Parameter(typeof(object));

            foreach (var item in keyProperties)
            {
                var propLeft = Expression.Property(Expression.Convert(paramLeft, clrType), item.Name);
                var propRight = Expression.Property(Expression.Convert(paramRight, clrType), item.Name);

                Expression expression = null;

                expression = GetPropertyCompareExpression(propLeft, propRight);

                if (compareExpression == null)
                {
                    compareExpression = expression;
                }
                else
                {
                    compareExpression = Expression.And(compareExpression, expression);
                }
            }

            var lambda = Expression.Lambda<Func<object, object, bool>>(compareExpression, paramLeft, paramRight);
            return lambda.Compile();
        }

        private IEnumerable<EntityMetadata> FlattenEntities()
        {
            foreach (var item in ChildEntities)
            {
                foreach (var child in item.FlattenEntities())
                {
                    yield return child;
                }

                yield return item;
            }

            yield return this;
        }
    }
}