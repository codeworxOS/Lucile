using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Lucile.Data.Metadata.Builder;

namespace Lucile.Data.Metadata
{
    public class EntityMetadata : MetadataElement, IEntityMetadata
    {
        private readonly IEntityValueAccessor _entityValueAccesssor;
        private readonly ImmutableList<ScalarProperty> _primaryKeys;

        internal EntityMetadata(ModelCreationScope scope, EntityMetadataBuilder builder)
            : base(builder.Name)
        {
            scope.AddEntity(builder.TypeInfo, this);
            this.ClrType = builder.TypeInfo.ClrType;

            var childListBuilder = ImmutableList.CreateBuilder<EntityMetadata>();
            var propertyListBuilder = ImmutableList.CreateBuilder<ScalarProperty>();
            var navigationListBuilder = ImmutableList.CreateBuilder<NavigationPropertyMetadata>();

            var properties = builder.Properties.OrderBy(p => builder.PrimaryKey.Contains(p.Name) ? builder.PrimaryKey.ToList().IndexOf(p.Name) : builder.PrimaryKey.Count).ThenBy(p => p.Name, StringComparer.Ordinal).ToList();

            foreach (var item in properties.Where(p => !p.IsExcluded))
            {
                propertyListBuilder.Add(item.ToProperty(this, builder.PrimaryKey.Contains(item.Name)));
            }

            Properties = propertyListBuilder.ToImmutable();

            if (builder.BaseEntity != null)
            {
                BaseEntity = scope.GetEntity(builder.BaseEntity.TypeInfo);
            }

            childListBuilder.AddRange(scope.GetChildEntities(builder.TypeInfo));
            ChildEntities = childListBuilder.ToImmutable();

            foreach (var item in builder.Navigations.Where(p => !p.IsExcluded))
            {
                navigationListBuilder.Add(item.ToNavigation(scope, this));
            }

            Navigations = navigationListBuilder.ToImmutable();

            var primaryKeys = this.GetProperties().Where(p => p.IsPrimaryKey);
            _primaryKeys = primaryKeys.ToImmutableList();
            PrimaryKeyCount = _primaryKeys.Count;

            if (ClrType != null)
            {
                _entityValueAccesssor = scope.GetEntityValueAccessor(this);

                PrimaryKeyType = _entityValueAccesssor.GetPrimaryKeyType();
            }
        }

        public EntityMetadata BaseEntity
        {
            get;
        }

        IEntityMetadata IEntityMetadata.BaseEntity => BaseEntity;

        public ImmutableList<EntityMetadata> ChildEntities
        {
            get;
        }

        public Type ClrType
        {
            get;
        }

        public int PrimaryKeyCount
        {
            get;
        }

        public Type PrimaryKeyType
        {
            get;
        }

        public IEntityValueAccessor ValueAccessor => _entityValueAccesssor;

        protected ImmutableList<NavigationPropertyMetadata> Navigations { get; }

        protected ImmutableList<ScalarProperty> Properties { get; }

        public PropertyMetadata this[string propertyName]
        {
            get
            {
                return (PropertyMetadata)GetProperties(true).FirstOrDefault(p => p.Name == propertyName) ?? GetNavigations().FirstOrDefault(p => p.Name == propertyName);
            }
        }

        public Dictionary<object, NavigationPropertyMetadata> FlattenChildren(object entity, int maxLevel = 0)
        {
            var children = new Dictionary<object, NavigationPropertyMetadata>();
            FlattenChildren(entity, children, this, 0, maxLevel);
            return children;
        }

        public IEnumerable<EntityMetadata> FlattenEntities()
        {
            foreach (var item in ChildEntities)
            {
                foreach (var child in item.FlattenEntities())
                {
                    yield return child;
                }
            }

            yield return this;
        }

        public IEnumerable<NavigationPropertyMetadata> GetNavigations()
        {
            if (BaseEntity != null)
            {
                foreach (var item in BaseEntity.GetNavigations())
                {
                    yield return item;
                }
            }

            foreach (var item in Navigations)
            {
                yield return item;
            }
        }

        IEnumerable<INavigationProperty> IEntityMetadata.GetNavigations() => GetNavigations();

        public object GetPrimaryKeyObject(object entity)
        {
            return _entityValueAccesssor.GetPrimaryKey(entity);
        }

        public IEnumerable<ScalarProperty> GetProperties(bool includeNoneClrProperties)
        {
            if (BaseEntity != null)
            {
                foreach (var item in BaseEntity.GetProperties(includeNoneClrProperties))
                {
                    yield return item;
                }
            }

            foreach (var item in Properties)
            {
                if (!includeNoneClrProperties && !item.HasClrProperty)
                {
                    continue;
                }

                yield return item;
            }
        }

        public IEnumerable<ScalarProperty> GetProperties()
        {
            return GetProperties(false);
        }

        IEnumerable<IScalarProperty> IEntityMetadata.GetProperties(bool includeNoneClrProperties) => GetProperties(includeNoneClrProperties);

        public bool IsOfType(object parameter) => _entityValueAccesssor.IsOfType(parameter);

        public bool IsPrimaryKeySet(object source)
        {
            foreach (var item in _primaryKeys)
            {
                if (!object.Equals(item.GetValue(source), item.Default))
                {
                    return true;
                }
            }

            return false;
        }

        public bool KeyEquals(object leftEntity, object rightEntity)
        {
            return _entityValueAccesssor.KeyEquals(leftEntity, rightEntity);
        }

        private static void FlattenChildren(object parent, IDictionary<object, NavigationPropertyMetadata> children, EntityMetadata entity, int level, int maxLevel = 0)
        {
            var concreteEntity = entity.FlattenEntities().First(p => p.IsOfType(parent));

            foreach (var item in concreteEntity.GetNavigations())
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
    }
}