using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Lucile.Data.Metadata.Builder;

namespace Lucile.Data.Metadata
{
    public class MetadataModel
    {
        internal MetadataModel(MetadataModelBuilder modelBuilder)
        {
            var scope = new ModelCreationScope(modelBuilder);
            var unordered = GetSorted(modelBuilder.Entities).Select(p => scope.GetEntity(p.TypeInfo.ClrType)).ToList();
            var targetList = unordered.Where(p => p.BaseEntity == null).OrderBy(p => p.Name).ToList();

            targetList.ForEach(p => unordered.Remove(p));

            while (unordered.Any())
            {
                var nextLayer = unordered.Where(p => targetList.Contains(p.BaseEntity)).ToList();
                foreach (var item in nextLayer.OrderByDescending(p => p.Name))
                {
                    unordered.Remove(item);
                    targetList.Insert(0, item);
                }
            }

            Entities = ImmutableList.CreateRange(targetList);
        }

        public ImmutableList<EntityMetadata> Entities { get; }

        public EntityMetadata GetEntityMetadata(object parameter)
        {
            return this.Entities.FirstOrDefault(p => p.IsOfType(parameter));
        }

        public EntityMetadata GetEntityMetadata<T>()
        {
            return GetEntityMetadata(typeof(T));
        }

        public EntityMetadata GetEntityMetadata(Type entityType)
        {
            return this.Entities.FirstOrDefault(p => p.ClrType.IsAssignableFrom(entityType));
        }

        private IEnumerable<EntityMetadataBuilder> GetSorted(IEnumerable<EntityMetadataBuilder> baseItems)
        {
            var baseList = baseItems.ToList();
            List<EntityMetadataBuilder> parents = null;

            while (baseList.Any())
            {
                parents = baseList.Where(p => parents == null ? p.BaseEntity == null : parents.Contains(p.BaseEntity)).ToList();
                foreach (var item in parents)
                {
                    baseList.Remove(item);
                    yield return item;
                }
            }
        }
    }
}