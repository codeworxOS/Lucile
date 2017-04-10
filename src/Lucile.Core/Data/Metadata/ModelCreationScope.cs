using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Lucile.Data.Metadata.Builder;

namespace Lucile.Data.Metadata
{
    internal class ModelCreationScope
    {
        private readonly Dictionary<Type, EntityMetadata> _entities;
        private readonly MetadataModelBuilder _modelBuilder;
        private readonly Dictionary<EntityMetadata, Dictionary<string, NavigationPropertyMetadata>> _navigationProperties;

        public ModelCreationScope(MetadataModelBuilder builder)
        {
            _modelBuilder = builder;
            _entities = new Dictionary<Type, EntityMetadata>();
            _navigationProperties = new Dictionary<EntityMetadata, Dictionary<string, NavigationPropertyMetadata>>();
            Entities = new ReadOnlyDictionary<Type, EntityMetadata>(_entities);
            NavigationProperties = new ReadOnlyDictionary<EntityMetadata, Dictionary<string, NavigationPropertyMetadata>>(_navigationProperties);
        }

        public IReadOnlyDictionary<Type, EntityMetadata> Entities { get; }

        public IReadOnlyDictionary<EntityMetadata, Dictionary<string, NavigationPropertyMetadata>> NavigationProperties { get; }

        public void AddEntity(Type clrType, EntityMetadata entityMetadata)
        {
            _entities.Add(clrType, entityMetadata);
            _navigationProperties.Add(entityMetadata, new Dictionary<string, NavigationPropertyMetadata>());
        }

        public void AddNavigationProperty(EntityMetadata entityType, string propertyName, NavigationPropertyMetadata navMetadata)
        {
            _navigationProperties[entityType].Add(propertyName, navMetadata);
        }

        public IEnumerable<EntityMetadata> GetChildEntities(Type type)
        {
            var items = this._modelBuilder.Entities.Where(p => !p.IsExcluded && p.BaseEntity?.TypeInfo.ClrType == type);
            foreach (var item in items)
            {
                EntityMetadata result;
                if (Entities.TryGetValue(item.TypeInfo.ClrType, out result))
                {
                    yield return result;
                }
                else
                {
                    yield return new EntityMetadata(this, _modelBuilder.Entity(item.TypeInfo.ClrType));
                }
            }
        }

        public EntityMetadata GetEntity(Type type)
        {
            EntityMetadata result;
            if (Entities.TryGetValue(type, out result))
            {
                return result;
            }

            var entityBuilder = _modelBuilder.Entity(type);

            var rootEntity = entityBuilder.BaseEntity;
            while (rootEntity?.BaseEntity != null)
            {
                rootEntity = rootEntity?.BaseEntity;
            }

            if (rootEntity != null)
            {
                var root = GetEntity(rootEntity.TypeInfo.ClrType);
                if (Entities.TryGetValue(type, out result))
                {
                    return result;
                }
            }

            return new EntityMetadata(this, entityBuilder);
        }
    }
}