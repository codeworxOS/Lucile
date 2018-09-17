using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;
using Lucile.Data.Metadata;
using Lucile.Data.Metadata.Builder;

namespace Lucile.EntityFramework
{
    public static class EntityFrameworkMetadataExtensions
    {
        public static void UseDbContext(this MetadataModelBuilder builder, DbContext context)
        {
            UseWorkspace(builder, ((IObjectContextAdapter)context).ObjectContext.MetadataWorkspace);
        }

        public static void UseWorkspace(this MetadataModelBuilder builder, MetadataWorkspace metadata)
        {
            var entityTypes = metadata
                .GetItems<EntityType>(DataSpace.CSpace);

            foreach (var entity in entityTypes)
            {
                var entityBuilder = builder.FromEntityType(metadata, entity);
            }
        }

        private static EntityMetadataBuilder FromEntityType(this MetadataModelBuilder builder, MetadataWorkspace metadata, EntityType entityType)
        {
            var objectItemCollection = (ObjectItemCollection)metadata.GetItemCollection(DataSpace.OSpace);
            var mapping = metadata.GetItems<EntityContainerMapping>(DataSpace.CSSpace)
                                    .Single()
                                    .EntitySetMappings
                                    .SelectMany(p => p.EntityTypeMappings);

            var objectTypes = metadata.GetItems<EntityType>(DataSpace.OSpace);

            var objectType = objectTypes.Single(p => p.Name == entityType.Name);
            var clrType = objectItemCollection.GetClrType(objectType);

            var entityBuilder = builder.Entity(clrType);
            if (entityType.BaseType != null)
            {
                var baseEntityBuilder = builder.FromEntityType(metadata, (EntityType)entityType.BaseType);
                entityBuilder.BaseEntity = baseEntityBuilder;
            }

            var propertyMappings = mapping.SelectMany(p => p.Fragments.SelectMany(x => x.PropertyMappings)).OfType<ScalarPropertyMapping>().ToList();

            var joined = from p in entityType.Properties.Where(p => p.DeclaringType == entityType)
                         join m in propertyMappings on p equals m.Property
                         select new { Property = p, Column = m.Column };

            foreach (var prop in joined)
            {
                var propBuilder = entityBuilder.Property(prop.Property.Name);
                propBuilder.Nullable = prop.Property.Nullable;
                if (prop.Column.IsStoreGeneratedComputed)
                {
                    propBuilder.ValueGeneration = AutoGenerateValue.Both;
                }
                else if (prop.Column.IsStoreGeneratedIdentity)
                {
                    propBuilder.ValueGeneration = AutoGenerateValue.OnInsert;
                }
            }

            if (entityType.BaseType == null)
            {
                foreach (var pk in entityType.KeyProperties)
                {
                    entityBuilder.PrimaryKey.Add(pk.Name);
                }
            }

            foreach (var nav in entityType.NavigationProperties.Where(p => p.DeclaringType == entityType))
            {
                NavigationPropertyBuilder navBuilder = null;

                var inverse = nav.ToEndMember.MetadataProperties.FirstOrDefault(p => p.Name == "ClrPropertyInfo")?.Value as PropertyInfo;

                var targetObjectType = objectTypes.Single(p => p.Name == nav.ToEndMember.GetEntityType().Name);
                var targetClrType = objectItemCollection.GetClrType(targetObjectType);

                string targetPropertyName = inverse?.Name;
                List<string> foreignKeys = new List<string>();

                var multiplicity = GetMultiplicity(nav.ToEndMember.RelationshipMultiplicity);
                var targetMultiplicity = GetMultiplicity(nav.FromEndMember.RelationshipMultiplicity);

                if (multiplicity != NavigationPropertyMultiplicity.Many)
                {
                    foreignKeys.AddRange(nav.GetDependentProperties().Select(p => p.Name));
                }

                navBuilder = entityBuilder.Navigation(nav.Name);

                navBuilder.Nullable = multiplicity == NavigationPropertyMultiplicity.ZeroOrOne;
                navBuilder.Multiplicity = multiplicity;
                navBuilder.Target = new ClrTypeInfo(targetClrType);
                navBuilder.TargetProperty = targetPropertyName;
                navBuilder.TargetMultiplicity = targetMultiplicity;
                navBuilder.ForeignKey = foreignKeys;
            }

            return entityBuilder;
        }

        private static NavigationPropertyMultiplicity GetMultiplicity(RelationshipMultiplicity relationshipMultiplicity)
        {
            switch (relationshipMultiplicity)
            {
                case RelationshipMultiplicity.ZeroOrOne:
                    return NavigationPropertyMultiplicity.ZeroOrOne;

                case RelationshipMultiplicity.One:
                    return NavigationPropertyMultiplicity.One;

                case RelationshipMultiplicity.Many:

                    return NavigationPropertyMultiplicity.Many;

                default:
                    throw new NotSupportedException("This should not happen");
            }
        }
    }
}