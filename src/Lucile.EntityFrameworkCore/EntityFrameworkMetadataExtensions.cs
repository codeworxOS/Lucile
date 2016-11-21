using System;
using System.Collections.Generic;
using System.Linq;
using Lucile.Data.Metadata;
using Lucile.Data.Metadata.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Lucile.EntityFrameworkCore
{
    public static class EntityFrameworkMetadataExtensions
    {
        public static void UseDbContext(this MetadataModelBuilder builder, DbContext context)
        {
            foreach (var entity in context.Model.GetEntityTypes())
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

            foreach (var prop in entityType.GetProperties().Where(p => p.DeclaringEntityType == entityType && !p.IsShadowProperty))
            {
                var propBuilder = entityBuilder.Property(prop.Name);
                propBuilder.Nullable = prop.IsNullable;
                propBuilder.IsIdentity = prop.ValueGenerated == ValueGenerated.OnAdd;
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
                var inverse = nav.FindInverse();
                Type targetType = nav.GetTargetType().ClrType;
                string targetPropertyName = inverse?.Name;
                List<string> foreignKeys = new List<string>();

                NavigationPropertyMultiplicity multiplicity = NavigationPropertyMultiplicity.One;
                NavigationPropertyMultiplicity targetMultiplicity = NavigationPropertyMultiplicity.One;
                if (nav.IsCollection())
                {
                    multiplicity = NavigationPropertyMultiplicity.Many;
                    targetMultiplicity = nav.ForeignKey.IsRequired ? NavigationPropertyMultiplicity.One : NavigationPropertyMultiplicity.ZeroOrOne;
                }
                else if (nav.IsDependentToPrincipal())
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