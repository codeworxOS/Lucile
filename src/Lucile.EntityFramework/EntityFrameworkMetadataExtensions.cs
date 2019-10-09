using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using Lucile.Data.Metadata;
using Lucile.Data.Metadata.Builder;
using Lucile.EntityFramework.Metadata;

namespace Lucile.EntityFramework
{
    public static class EntityFrameworkMetadataExtensions
    {
        public static DbModelBuilder AddDefaultValues(this DbModelBuilder modelBuilder)
        {
            var convention = new AttributeToColumnAnnotationConvention<DefaultValueAttribute, string>(DefaultValueAnnotation.AnnotationName, (p, attributes) => attributes.SingleOrDefault().Value.ToString());

            modelBuilder.Conventions.Add(convention);

            return modelBuilder;
        }

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

                propBuilder.HasDefaultValue = prop.Column.MetadataProperties.Any(p => p.IsAnnotation && p.Name.EndsWith(":" + DefaultValueAnnotation.AnnotationName, StringComparison.Ordinal));

                if (propBuilder is TextPropertyBuilder textBuilder)
                {
                    textBuilder.MaxLength = prop.Property.MaxLength;
                }
                else if (propBuilder is BlobPropertyBuilder blobPropertyBuilder)
                {
                    blobPropertyBuilder.MaxLength = prop.Property.MaxLength;
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

                ////var navpropMatch = from l in item.EntityMetadata.Properties.OfType<NavigationPropertyMetadata>()
                ////                   join r in item.EntityType.NavigationProperties on l.Name equals r.Name
                ////                   select new { Left = l, Right = r };

                ////foreach (var npmatch in navpropMatch)
                ////{
                ////    var test = entityTypes.SelectMany(p => p.NavigationProperties)
                ////                          .FirstOrDefault(p => p.ToEndMember == npmatch.Right.FromEndMember);

                var inverse = nav.ToEndMember.GetEntityType().NavigationProperties.FirstOrDefault(p => p.FromEndMember == nav.ToEndMember);

                ////var inverse = nav.ToEndMember.MetadataProperties.FirstOrDefault(p => p.Name == "ClrPropertyInfo")?.Value as PropertyInfo;

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