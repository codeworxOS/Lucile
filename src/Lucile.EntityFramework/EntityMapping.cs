using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;

namespace Lucile.EntityFramework
{
    public class EntityMapping
    {
        public EntityMapping(DbContext db)
        {
            var metadata = ((IObjectContextAdapter)db).ObjectContext.MetadataWorkspace;

            this.TypeMappings = new List<TypeMapping>();

            // Conceptual part of the model has info about the shape of our entity classes
            var conceptualContainer = metadata.GetItems<EntityContainer>(DataSpace.CSpace).Single();

            // Storage part of the model has info about the shape of our tables
            var storeContainer = metadata.GetItems<EntityContainer>(DataSpace.SSpace).Single();

            var mappingItemCollection = (StorageMappingItemCollection)metadata.GetItemCollection(DataSpace.CSSpace);
            var typeMappings = mappingItemCollection.GetItems<EntityContainerMapping>()
                            .SelectMany(p => p.EntitySetMappings)
                            .SelectMany(p => p.EntityTypeMappings);

            // Loop thru each entity type in the model
            foreach (var entity in metadata.GetItems<EntityType>(DataSpace.CSpace).Where(p => !p.Abstract))
            {
                var typeMapping = new TypeMapping
                {
                    TableMappings = new List<TableMapping>()
                };

                var entityType = metadata.GetClrTypeFromCSpaceType(entity);

                if (entityType == null)
                {
                    continue;
                }

                typeMapping.EntityType = entityType;
                this.TypeMappings.Add(typeMapping);

                /* Get the mapping fragments for this type
                  (types may have mutliple fragments if 'Entity Splitting' is used) */

                AddTableMappings(typeMappings, entity, typeMapping);
            }
        }

        public List<TypeMapping> TypeMappings { get; set; }

        private static void AddTableMappings(IEnumerable<EntityTypeMapping> typeMappings, EntityType entity, TypeMapping typeMapping)
        {
            var baseType = entity.BaseType as EntityType;
            if (baseType != null)
            {
                AddTableMappings(typeMappings, baseType, typeMapping);
            }

            var mappingFragments = typeMappings.Where(p => p.EntityType == entity || p.IsOfEntityTypes.Contains(entity))
                    .OrderBy(p => p.IsHierarchyMapping ? 0 : 1)
                    .SelectMany(p => p.Fragments);

            foreach (var mapping in mappingFragments)
            {
                var tableName = mapping.StoreEntitySet.Table;
                var schemaName = mapping.StoreEntitySet.Schema;

                TableMapping tableMapping = typeMapping.TableMappings.FirstOrDefault(p => p.TableName == tableName && p.SchemaName == schemaName);

                if (tableMapping != null)
                {
                    tableMapping.Conditions.Clear();
                }
                else
                {
                    tableMapping = new TableMapping
                    {
                        TableName = tableName,
                        SchemaName = schemaName,
                        PropertyMappings = new List<PropertyMapping>(),
                        Conditions = new List<ConditionMapping>()
                    };
                    typeMapping.TableMappings.Add(tableMapping);
                }

                foreach (var item in mapping.Conditions)
                {
                    var nullCondition = item as IsNullConditionMapping;
                    var valueCondition = item as ValueConditionMapping;
                    ConditionMapping condition = null;
                    if (nullCondition != null)
                    {
                        condition = new NullConditionMapping { Value = nullCondition.IsNull };
                    }
                    else if (valueCondition != null)
                    {
                        condition = new ConditionMapping { Value = valueCondition.Value };
                    }

                    condition.ColumnName = item.Column.Name;
                    condition.ColumnType = item.Column.PrimitiveType.ClrEquivalentType;
                    if (item.Property != null)
                    {
                        condition.Property = typeMapping.EntityType.GetProperty(item.Property.Name);
                    }

                    tableMapping.Conditions.Add(condition);
                }

                // Find the property-to-column mappings
                var propertyMappings = mapping
                        .PropertyMappings.OfType<ScalarPropertyMapping>();

                foreach (var propertyMapping in propertyMappings)
                {
                    // Find the property and column being mapped
                    var propertyName = propertyMapping.Property.Name;
                    var columnName = propertyMapping.Column.Name;

                    if (tableMapping.PropertyMappings.Any(p => p.Property.Name == propertyName))
                    {
                        continue;
                    }

                    tableMapping.PropertyMappings.Add(new PropertyMapping
                    {
                        Property = typeMapping.EntityType.GetProperty(propertyName),
                        ColumnName = columnName
                    });
                }
            }
        }

        public class ConditionMapping : PropertyMapping
        {
            public Type ColumnType { get; set; }

            public object Value { get; set; }
        }

        public class NullConditionMapping : ConditionMapping
        {
        }

        public class PropertyMapping
        {
            public string ColumnName { get; set; }

            public PropertyInfo Property { get; set; }
        }

        public class TableMapping
        {
            public List<ConditionMapping> Conditions { get; set; }

            public List<PropertyMapping> PropertyMappings { get; set; }

            public string SchemaName { get; set; }

            public string TableName { get; set; }
        }

        public class TypeMapping
        {
            public Type EntityType { get; set; }

            public List<TableMapping> TableMappings { get; set; }
        }
    }
}