using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public EntityMetadata GetEntity(Type type)
        {
            EntityMetadata result;
            if (Entities.TryGetValue(type, out result))
            {
                return result;
            }

            return new EntityMetadata(this, _modelBuilder.Entity(type));
        }
    }
}