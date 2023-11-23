﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Lucile.Core.Data.Metadata.Expressions;

namespace Lucile.Data.Metadata.Builder
{
    [DataContract(IsReference = true)]
    [ProtoBuf.ProtoContract(AsReferenceDefault = true)]
    public class MetadataModelBuilder
    {
        private ConcurrentDictionary<EntityKey, EntityMetadataBuilder> _entities;

        public MetadataModelBuilder()
        {
            _entities = new ConcurrentDictionary<EntityKey, EntityMetadataBuilder>();
        }

        [DataMember(Order = 1)]
        public IEnumerable<EntityMetadataBuilder> Entities
        {
            get
            {
                return _entities.Any() ? _entities.Values.ToList() : null;
            }

            set
            {
                var values = value.ToDictionary(p => EntityKey.Get(p.TypeInfo), p => p);
                _entities = new ConcurrentDictionary<EntityKey, EntityMetadataBuilder>(values);
            }
        }

        public EntityMetadataBuilder<TEntity> Entity<TEntity>()
            where TEntity : class
        {
            return Entity<TEntity>(false);
        }

        public EntityMetadataBuilder<TEntity> Entity<TEntity>(bool addBase)
            where TEntity : class
        {
            return new EntityMetadataBuilder<TEntity>(Entity(EntityKey.Get<TEntity>(), addBase));
        }

        public EntityMetadataBuilder Entity(Type clrType)
        {
            return Entity(clrType, false);
        }

        public EntityMetadataBuilder Entity(ClrTypeInfo typeInfo)
        {
            return Entity(typeInfo, false);
        }

        public EntityMetadataBuilder Entity(Type clrType, bool addBase)
        {
            return Entity(EntityKey.Get(new ClrTypeInfo(clrType)), addBase);
        }

        public EntityMetadataBuilder Entity(ClrTypeInfo typeInfo, bool addBase)
        {
            return Entity(EntityKey.Get(typeInfo), addBase);
        }

        public MetadataModelBuilder Exclude<TEntity>()
            where TEntity : class
        {
            Entity<TEntity>().IsExcluded = true;
            return this;
        }

        public MetadataModelBuilder Exclude(Type entityType)
        {
            Entity(entityType).IsExcluded = true;
            return this;
        }

        public MetadataModelBuilder FromModel(MetadataModel model)
        {
            foreach (var item in model.Entities)
            {
                var entity = this.Entity(item.ClrType);
                if (item.BaseEntity != null)
                {
                    entity.BaseEntity = this.Entity(item.BaseEntity.ClrType);
                }

                foreach (var prop in item.GetProperties().Where(p => p.Entity == item))
                {
                    var scalar = entity.Property(prop.Name, prop.PropertyType);
                    scalar.CopyFrom(prop);
                    if (prop.IsPrimaryKey)
                    {
                        entity.PrimaryKey.Add(prop.Name);
                    }
                }

                foreach (var nav in item.GetNavigations().Where(p => p.Entity == item))
                {
                    var navigation = entity.Navigation(nav.Name);
                    navigation.CopyFrom(nav);
                }
            }

            return this;
        }

        public MetadataModel ToModel()
        {
            return ToModel(new ExpressionValueAccessorFactory());
        }

        public MetadataModel ToModel(IValueAccessorFactory valueAccessorFactory)
        {
            return new MetadataModel(this, valueAccessorFactory);
        }

        protected EntityMetadataBuilder Entity(EntityKey entityKey)
        {
            return Entity(entityKey, false);
        }

        protected EntityMetadataBuilder Entity(EntityKey entityKey, bool addBase)
        {
            return _entities.GetOrAdd(entityKey, p => p.GetBuilder(this, addBase));
        }

        protected class EntityKey
        {
            private static readonly ConcurrentDictionary<ClrTypeInfo, EntityKey> _keys;
            private readonly ClrTypeInfo _typeInfo;

            static EntityKey()
            {
                _keys = new ConcurrentDictionary<ClrTypeInfo, EntityKey>();
            }

            private EntityKey(ClrTypeInfo typeInfo)
            {
                _typeInfo = typeInfo;
            }

            public static EntityKey Get(ClrTypeInfo clrTypeInfo)
            {
                return _keys.GetOrAdd(clrTypeInfo, p => new EntityKey(p));
            }

            public static EntityKey Get<TEntity>()
            {
                return Get(new ClrTypeInfo(typeof(TEntity)));
            }

            public EntityMetadataBuilder GetBuilder(MetadataModelBuilder modelBuilder)
            {
                return GetBuilder(modelBuilder, false);
            }

            public EntityMetadataBuilder GetBuilder(MetadataModelBuilder modelBuilder, bool addBase)
            {
                var result = new EntityMetadataBuilder(modelBuilder) { TypeInfo = _typeInfo.Clone() };

                if (addBase && result.BaseEntity == null)
                {
                    if (result.TypeInfo.ClrType.BaseType != typeof(object))
                    {
                        result.BaseEntity = modelBuilder.Entity(result.TypeInfo.ClrType.BaseType);
                    }
                }

                return result;
            }
        }
    }
}