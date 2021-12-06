using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Lucile.Data.Metadata.Builder;

namespace Lucile.Data.Metadata
{
    internal class ModelCreationScope
    {
        private readonly Dictionary<ClrTypeInfo, EntityMetadata> _entities;
        private readonly IValueAccessorFactory _valueAccessorFactory;
        private readonly MetadataModelBuilder _modelBuilder;
        private readonly Dictionary<EntityMetadata, Dictionary<string, NavigationPropertyMetadata>> _navigationProperties;

        public ModelCreationScope(MetadataModelBuilder builder, IValueAccessorFactory valueAccessorFactory)
        {
            _valueAccessorFactory = valueAccessorFactory;
            _modelBuilder = builder;
            _entities = new Dictionary<ClrTypeInfo, EntityMetadata>();
            _navigationProperties = new Dictionary<EntityMetadata, Dictionary<string, NavigationPropertyMetadata>>();
            Entities = new ReadOnlyDictionary<ClrTypeInfo, EntityMetadata>(_entities);
            NavigationProperties = new ReadOnlyDictionary<EntityMetadata, Dictionary<string, NavigationPropertyMetadata>>(_navigationProperties);
        }

        public IReadOnlyDictionary<ClrTypeInfo, EntityMetadata> Entities { get; }

        public IReadOnlyDictionary<EntityMetadata, Dictionary<string, NavigationPropertyMetadata>> NavigationProperties { get; }

        public void AddEntity(ClrTypeInfo typeInfo, EntityMetadata entityMetadata)
        {
            _entities.Add(typeInfo, entityMetadata);
            _navigationProperties.Add(entityMetadata, new Dictionary<string, NavigationPropertyMetadata>());
        }

        public void AddNavigationProperty(EntityMetadata entityType, string propertyName, NavigationPropertyMetadata navMetadata)
        {
            _navigationProperties[entityType].Add(propertyName, navMetadata);
        }

        public IEnumerable<EntityMetadata> GetChildEntities(ClrTypeInfo typeInfo)
        {
            var items = this._modelBuilder.Entities.Where(p => !p.IsExcluded && p.BaseEntity?.TypeInfo == typeInfo);
            foreach (var item in items)
            {
                EntityMetadata result;
                if (Entities.TryGetValue(item.TypeInfo, out result))
                {
                    yield return result;
                }
                else
                {
                    yield return new EntityMetadata(this, _modelBuilder.Entity(item.TypeInfo));
                }
            }
        }

        public EntityMetadata GetEntity(ClrTypeInfo typeInfo)
        {
            EntityMetadata result;
            if (Entities.TryGetValue(typeInfo, out result))
            {
                return result;
            }

            var entityBuilder = _modelBuilder.Entity(typeInfo);

            var rootEntity = entityBuilder.BaseEntity;
            while (rootEntity?.BaseEntity != null)
            {
                rootEntity = rootEntity?.BaseEntity;
            }

            if (rootEntity != null)
            {
                var root = GetEntity(rootEntity.TypeInfo);
                if (Entities.TryGetValue(typeInfo, out result))
                {
                    return result;
                }
            }

            return new EntityMetadata(this, entityBuilder);
        }

        public IEntityValueAccessor GetEntityValueAccessor(IEntityMetadata entityMetadata)
        {
            return _valueAccessorFactory.GetAccessor(entityMetadata);
        }
    }
}