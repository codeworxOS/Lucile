using System;
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
            var unorderd = modelBuilder.Entities.Select(p => scope.GetEntity(p.TypeInfo.ClrType)).ToList();

            Entities = ImmutableList.CreateRange(unorderd);
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
    }
}