using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Lucile.Data.Metadata.Builder;
using Lucile.Data.Metadata.Builder.Convention;

namespace Lucile.Data.Metadata
{
    public static class DefaultMetadataModelExtensions
    {
        public static MetadataModelBuilder ApplyConventions(this MetadataModelBuilder builder, ConventionCollection conventions = null)
        {
            conventions = conventions ?? ConventionCollection.DefaultConventions;

            var structureConventions = conventions.OfType<IStructureConvention>().ToList();
            var oldNavigationProperties = builder.Entities.SelectMany(p => p.Navigations).ToList();
            var added = new Dictionary<Type, List<NavigationPropertyBuilder>>();

            ResolveNavigations(builder, structureConventions, builder.Entities.Select(p => p.TypeInfo.ClrType).ToList());

            foreach (var entity in builder.Entities.Where(p => !p.IsExcluded))
            {
                foreach (var item in structureConventions.SelectMany(p => p.GetScalarProperties(entity.TypeInfo.ClrType)).Distinct())
                {
                    entity.Property(item);
                }
            }

            foreach (var entity in builder.Entities.Where(p => !p.IsExcluded))
            {
                foreach (var item in structureConventions.SelectMany(p => p.GetNavigations(entity.TypeInfo.ClrType)).Distinct())
                {
                    var nav = entity.Navigation(item.Key);

                    if (!oldNavigationProperties.Contains(nav))
                    {
                        if (!added.ContainsKey(entity.TypeInfo.ClrType))
                        {
                            added.Add(entity.TypeInfo.ClrType, new List<NavigationPropertyBuilder>());
                        }

                        added[entity.TypeInfo.ClrType].Add(nav);
                    }
                }
            }

            var entityConventions = conventions.OfType<IEntityConvention>().ToList();

            foreach (var item in builder.Entities)
            {
                entityConventions.ForEach(p => p.Apply(item));
            }

            foreach (var item in added)
            {
                var entity = builder.Entity(item.Key);
                foreach (var nav in item.Value.Where(p => p.Multiplicity != NavigationPropertyMultiplicity.Many && !p.ForeignKey.Any()))
                {
                    var fk = entity.Properties.Where(p => p.Name.Equals($"{nav.Name}Id", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    if (fk != null)
                    {
                        nav.Multiplicity = fk.Nullable ? NavigationPropertyMultiplicity.ZeroOrOne : NavigationPropertyMultiplicity.One;
                        nav.ForeignKey.Add(fk.Name);
                    }
                }
            }

            foreach (var item in added)
            {
                foreach (var nav in item.Value.ToList())
                {
                    var target = builder.Entity(nav.Target.ClrType).Navigations
                        .FirstOrDefault(p => p.Target.ClrType.IsAssignableFrom(item.Key) && p.TargetProperty == nav.Name);
                    if (target != null)
                    {
                        nav.Multiplicity = target.TargetMultiplicity;
                        nav.TargetMultiplicity = target.Multiplicity;
                        nav.TargetProperty = target.Name;
                        item.Value.Remove(nav);
                    }
                }

                foreach (var nav in item.Value.GroupBy(p => new { Type = p.Target.ClrType, IsCollection = p.Multiplicity == NavigationPropertyMultiplicity.Many }))
                {
                    if (nav.Count() > 1)
                    {
                        throw new ModelBuilderValidationExcpetion($"Multiple NavigationProperties for same target entity {nav.Key.Type} found. Use .Navigation() configuration on EntityMetadataBuilder.");
                    }

                    List<NavigationPropertyBuilder> target = new List<NavigationPropertyBuilder>();

                    if (added.ContainsKey(nav.First().Target.ClrType))
                    {
                        if (nav.First().Multiplicity == NavigationPropertyMultiplicity.Many)
                        {
                            target = added[nav.First().Target.ClrType].Where(p => p.Target.ClrType == item.Key && p.Multiplicity != NavigationPropertyMultiplicity.Many).ToList();
                        }
                        else
                        {
                            target = added[nav.First().Target.ClrType].Where(p => p.Target.ClrType == item.Key && p.Multiplicity == NavigationPropertyMultiplicity.Many).ToList();
                        }
                    }

                    if (!target.Any())
                    {
                        nav.First().TargetMultiplicity = nav.First().Multiplicity == NavigationPropertyMultiplicity.Many ? NavigationPropertyMultiplicity.ZeroOrOne : NavigationPropertyMultiplicity.Many;
                    }
                    else if (target.Count == 1)
                    {
                        nav.First().TargetMultiplicity = target.First().Multiplicity;
                        nav.First().TargetProperty = target.First().Name;
                    }
                    else
                    {
                        throw new ModelBuilderValidationExcpetion($"Multiple NavigationProperties for same target entity {item.Key} found. Use .Navigation() configuration on EntityMetadataBuilder.");
                    }
                }
            }

            return builder;
        }

        private static void ResolveNavigations(MetadataModelBuilder builder, IEnumerable<IStructureConvention> conventions, IEnumerable<Type> types)
        {
            var targetTypes = types.SelectMany(p => conventions.SelectMany(x => x.GetNavigations(p)?.Values ?? Enumerable.Empty<Type>())).Distinct();

            var missingTypes = targetTypes.Except(builder.Entities.Select(p => p.TypeInfo.ClrType)).ToList();

            if (missingTypes.Any())
            {
                foreach (var item in missingTypes)
                {
                    builder.Entity(item);
                }

                ResolveNavigations(builder, conventions, missingTypes);
            }
        }
    }
}