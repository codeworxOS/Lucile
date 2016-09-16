using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using Lucile.Reflection;

namespace Lucile.Data.Metadata
{
    [DataContract(IsReference = true)]
    [ProtoBuf.ProtoContract(AsReferenceDefault = true)]
    public class EntityMetadata : MetadataElement
    {
        private EntityMetadata _baseEntity;
        private Func<object, bool> _checkTypeDelegate;
        private List<EntityMetadata> _childEntities;
        private Type _clrType = null;
        private string _clrTypeName = null;
        private Func<object, object, bool> _keyEqualsDelegate;
        private ReadOnlyCollection<EntityMetadata> _readOnlyChild;

        public EntityMetadata()
        {
            this.Properties = new List<PropertyMetadata>();
        }

        [DataMember(Order = 2)]
        public EntityMetadata BaseEntity
        {
            get
            {
                return _baseEntity;
            }

            set
            {
                if (_baseEntity != null && _baseEntity.ChildEntities.Contains(this))
                {
                    _baseEntity._childEntities.Remove(this);
                }

                if (value != null && !value.ChildEntities.Contains(this))
                {
                    value._childEntities.Add(this);
                }

                _baseEntity = value;
            }
        }

        [IgnoreDataMember]
        public IEnumerable<EntityMetadata> ChildEntities
        {
            get
            {
                if (_childEntities == null)
                {
                    _childEntities = new List<EntityMetadata>();
                    _readOnlyChild = new ReadOnlyCollection<EntityMetadata>(this._childEntities);
                }

                return _readOnlyChild;
            }
        }

        [IgnoreDataMember]
        public Type ClrType
        {
            get
            {
                if (_clrType == null)
                {
                    _clrType = TypeResolver.GetType(this.ClrTypeName);
                }

                return _clrType;
            }

            set
            {
                this.ClrTypeName = value != null ? value.AssemblyQualifiedName : null;
                _clrType = value;
            }
        }

        [DataMember(Order = 1)]
        public string ClrTypeName
        {
            get
            {
                return _clrTypeName;
            }

            set
            {
                this._clrTypeName = value;
                this._clrType = null;
            }
        }

        /// <summary>
        /// Liefert eine Liste der Properties
        /// </summary>
        [DataMember(Order = 3)]
        [ProtoBuf.ProtoMember(3, AsReference = true)]
        public ICollection<PropertyMetadata> Properties { get; private set; }

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

        public EntityKey GetEntityKey(object entity)
        {
            if (!this.IsOfType(entity))
            {
                throw new ArgumentException(string.Format("The given entity is not of type {0}", this.ClrType), "entity");
            }

            var pks = this.Properties.OfType<ScalarProperty>().Where(p => p.IsPrimaryKey).ToList();

            var key = new EntityKey() { EntityType = this.ClrType };

            if (pks.Count == 1)
            {
                key.Key = pks.First().GetValue(entity);
            }
            else
            {
                var multiValueKey = new MultiValueKey();
                foreach (var item in pks)
                {
                    var value = item.GetValue(entity);
                    multiValueKey.Values.Add(item.Name, value);
                }

                key.Key = multiValueKey;
            }

            return key;
        }

        public bool IsOfType(object parameter)
        {
            if (_checkTypeDelegate == null)
            {
                var parameterExpresssion = Expression.Parameter(typeof(object));

                var isExpression = Expression.TypeIs(parameterExpresssion, this.ClrType);
                var lambda = Expression.Lambda<Func<object, bool>>(isExpression, parameterExpresssion);

                this._checkTypeDelegate = lambda.Compile();
            }

            return _checkTypeDelegate(parameter);
        }

        public bool KeyEquals(object left, object right)
        {
            if (this._keyEqualsDelegate == null)
            {
                Expression compareExpression = null;
                ParameterExpression paramLeft = Expression.Parameter(typeof(object));
                ParameterExpression paramRight = Expression.Parameter(typeof(object));

                foreach (var item in this.Properties.OfType<ScalarProperty>().Where(p => p.IsPrimaryKey))
                {
                    var propLeft = Expression.Property(Expression.Convert(paramLeft, this.ClrType), item.Name);
                    var propRight = Expression.Property(Expression.Convert(paramRight, this.ClrType), item.Name);

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
                this._keyEqualsDelegate = lambda.Compile();
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