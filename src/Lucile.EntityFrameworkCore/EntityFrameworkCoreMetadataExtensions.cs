using System;
using System.Collections.Generic;
using System.Linq;
using Lucile.Data.Metadata;
using Lucile.Data.Metadata.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Lucile.EntityFrameworkCore
{
    public static class EntityFrameworkCoreMetadataExtensions
    {
        public static void UseDbContext(this MetadataModelBuilder builder, DbContext context)
        {
            UseDbModel(builder, context.Model);
        }

        public static void UseDbModel(this MetadataModelBuilder builder, IModel model)
        {
            var entities = model.GetEntityTypes().ToList();

            entities = entities.Where(p => p.FindPrimaryKey() != null).ToList();

            foreach (var entity in entities)
            {
                var entityBuilder = builder.FromEntityType(entity);
            }
        }

        private static EntityMetadataBuilder FromEntityType(this MetadataModelBuilder builder, IEntityType entityType)
        {
            var entityBuilder = builder.Entity(entityType.ClrType);
            if (entityType.BaseType != null)
            {
                var baseEntityBuilder = builder.FromEntityType(entityType.BaseType);
                entityBuilder.BaseEntity = baseEntityBuilder;
            }

            var properties = entityType.GetProperties().Where(p => p.DeclaringEntityType == entityType);

            foreach (var prop in properties)
            {
                var propBuilder = entityBuilder.Property(prop.Name, prop.ClrType);
                propBuilder.Nullable = prop.IsNullable;
                if (prop.ValueGenerated == ValueGenerated.OnAdd)
                {
                    propBuilder.ValueGeneration = AutoGenerateValue.OnInsert;
                }
                else if (prop.ValueGenerated == ValueGenerated.OnAddOrUpdate)
                {
                    propBuilder.ValueGeneration = AutoGenerateValue.Both;
                }
#if NETSTANDARD2_0
                else if (prop.ValueGenerated == ValueGenerated.OnUpdate)
                {
                    propBuilder.ValueGeneration = AutoGenerateValue.Both;
                }
#endif

                propBuilder.HasDefaultValue = (prop.FindAnnotation("Relational:DefaultValue") ?? prop.FindAnnotation("Relational:DefaultValueSql")) != null;

                if (propBuilder is TextPropertyBuilder textBuilder)
                {
                    textBuilder.MaxLength = prop.GetMaxLength();
                }
                else if (propBuilder is BlobPropertyBuilder blobPropertyBuilder)
                {
                    blobPropertyBuilder.MaxLength = prop.GetMaxLength();
                }
            }

            if (entityType.BaseType == null)
            {
                foreach (var pk in entityType.FindPrimaryKey().Properties)
                {
                    entityBuilder.PrimaryKey.Add(pk.Name);
                }
            }

            foreach (var nav in entityType.GetNavigations().Where(p => p.DeclaringEntityType == entityType))
            {
                NavigationPropertyBuilder navBuilder = null;
#if EF3
                var inverse = nav.FindInverse();
                Type targetType = nav.GetTargetType().ClrType;
#else
                var inverse = nav.Inverse;
                Type targetType = nav.TargetEntityType.ClrType;
#endif
                string targetPropertyName = inverse?.Name;
                List<string> foreignKeys = new List<string>();

                NavigationPropertyMultiplicity multiplicity = NavigationPropertyMultiplicity.One;
                NavigationPropertyMultiplicity targetMultiplicity = NavigationPropertyMultiplicity.One;
#if EF3
                if (nav.IsCollection())
#else
                if (nav.IsCollection)
#endif
                {
                    multiplicity = NavigationPropertyMultiplicity.Many;
                    targetMultiplicity = nav.ForeignKey.IsRequired ? NavigationPropertyMultiplicity.One : NavigationPropertyMultiplicity.ZeroOrOne;
                }
#if EF3
                else if (nav.IsDependentToPrincipal())
#else
                else if (nav.IsOnDependent)
#endif
                {
                    multiplicity = nav.ForeignKey.IsRequired ? NavigationPropertyMultiplicity.One : NavigationPropertyMultiplicity.ZeroOrOne;
                    targetMultiplicity = nav.ForeignKey.IsUnique ? NavigationPropertyMultiplicity.ZeroOrOne : NavigationPropertyMultiplicity.Many;

                    foreignKeys = nav.ForeignKey.Properties.Select(p => p.Name).ToList();
                }
                else
                {
                    multiplicity = NavigationPropertyMultiplicity.ZeroOrOne;
                    targetMultiplicity = NavigationPropertyMultiplicity.One;
                }

                navBuilder = entityBuilder.Navigation(nav.Name);

                navBuilder.Nullable = multiplicity == NavigationPropertyMultiplicity.ZeroOrOne;
                navBuilder.Multiplicity = multiplicity;
                navBuilder.Target = new ClrTypeInfo(targetType);
                navBuilder.TargetProperty = targetPropertyName;
                navBuilder.TargetMultiplicity = targetMultiplicity;
                navBuilder.ForeignKey = foreignKeys;
            }

            return entityBuilder;
        }
    }
}