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
        public MetadataModel(IEnumerable<EntityMetadata> entities)
        {
            Entities = ImmutableList.CreateRange(entities);
        }

        internal MetadataModel(MetadataModelBuilder modelBuilder, IValueAccessorFactory valueAccessorFactory)
        {
            var scope = new ModelCreationScope(modelBuilder, valueAccessorFactory);
            var unordered = GetSorted(modelBuilder.Entities).Where(p => !p.IsExcluded).Select(p => scope.GetEntity(p.TypeInfo)).ToList();
            var targetList = unordered.Where(p => p.BaseEntity == null).OrderBy(p => p.Name, StringComparer.Ordinal).ToList();

            targetList.ForEach(p => unordered.Remove(p));

            while (unordered.Any())
            {
                var nextLayer = unordered.Where(p => targetList.Contains(p.BaseEntity)).ToList();
                foreach (var item in nextLayer.OrderByDescending(p => p.Name, StringComparer.Ordinal))
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
            for (int i = 0; i < Entities.Count; i++)
            {
                if (Entities[i].IsOfType(parameter))
                {
                    return Entities[i];
                }
            }

            return null;
        }

        public EntityMetadata GetEntityMetadata<T>()
        {
            return GetEntityMetadata(typeof(T));
        }

        public EntityMetadata GetEntityMetadata(Type entityType)
        {
            return this.Entities.FirstOrDefault(p => p.ClrType.IsAssignableFrom(entityType));
        }

        public IEnumerable<EntityMetadata> SortedByDependency()
        {
            List<EntityMetadata> ordered = new List<EntityMetadata>();
            List<EntityMetadata> unordered = Entities.ToList();

            int previousCount = 0;

            while (previousCount != unordered.Count && unordered.Any())
            {
                previousCount = unordered.Count;

                var loose = unordered.Where(p => !(unordered.Except(new[] { p })
                                                    .SelectMany(x => x.GetNavigations())
                                                    .Any(x => x.TargetEntity == p && IsPrincipalEnd(x))
                                                  ||
                                                    p.GetNavigations().Any(x => unordered.Except(new[] { p }).Contains(x.TargetEntity) && !IsPrincipalEnd(x))))
                                                    .ToList();

                foreach (var item in loose)
                {
                    ordered.Add(item);
                    unordered.Remove(item);
                }
            }

            if (unordered.Any())
            {
                throw new NotSupportedException("Circular references detected.");
            }

            return ordered;
        }

        private IEnumerable<EntityMetadataBuilder> GetSorted(IEnumerable<EntityMetadataBuilder> entities)
        {
            return entities.OrderBy(p => p.BaseEntity == null && entities.Any(x => x.BaseEntity == p) ? 0 : 1).ThenBy(p => p.Name, StringComparer.Ordinal);
        }

        private bool IsPrincipalEnd(NavigationPropertyMetadata prop)
        {
            switch (prop.Multiplicity)
            {
                case NavigationPropertyMultiplicity.Many:
                    return prop.TargetMultiplicity != NavigationPropertyMultiplicity.Many;

                case NavigationPropertyMultiplicity.One:
                    return false;

                case NavigationPropertyMultiplicity.ZeroOrOne:
                    return prop.TargetMultiplicity == NavigationPropertyMultiplicity.One;
            }

            return false;
        }
    }
}