using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;

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
                var values = value.ToDictionary(p => EntityKey.Get(p.TypeInfo.ClrType), p => p);
                _entities = new ConcurrentDictionary<EntityKey, EntityMetadataBuilder>(values);
            }
        }

        public EntityMetadataBuilder<TEntity> Entity<TEntity>()
            where TEntity : class
        {
            return new EntityMetadataBuilder<TEntity>(Entity(EntityKey.Get<TEntity>()));
        }

        public EntityMetadataBuilder Entity(Type clrType)
        {
            return Entity(EntityKey.Get(clrType));
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
            return new MetadataModel(this);
        }

        protected EntityMetadataBuilder Entity(EntityKey entityKey)
        {
            return _entities.GetOrAdd(entityKey, p => p.GetBuilder(this));
        }

        protected abstract class EntityKey
        {
            private static readonly ConcurrentDictionary<Type, Func<EntityKey>> _keys;

            static EntityKey()
            {
                _keys = new ConcurrentDictionary<Type, Func<Builder.MetadataModelBuilder.EntityKey>>();
            }

            public static EntityKey Get(Type clrType)
            {
                return _keys.GetOrAdd(clrType, CreateEntityKeyDelegate)();
            }

            public static EntityKey Get<TEntity>()
            {
                return EntityKey<TEntity>.Key;
            }

            public abstract EntityMetadataBuilder GetBuilder(MetadataModelBuilder modelBuilder);

            private static Func<EntityKey> CreateEntityKeyDelegate(Type entityType)
            {
                var body = Expression.Property(null, typeof(EntityKey<>).MakeGenericType(entityType).GetProperty("Key", BindingFlags.Public | BindingFlags.Static));

                return Expression.Lambda<Func<EntityKey>>(body).Compile();
            }
        }

        private class EntityKey<TEntity> : EntityKey
        {
            static EntityKey()
            {
                Key = new EntityKey<TEntity>();
            }

            private EntityKey()
            {
            }

            public static EntityKey Key { get; }

            public override EntityMetadataBuilder GetBuilder(MetadataModelBuilder modelBuilder)
            {
                return new EntityMetadataBuilder(modelBuilder) { TypeInfo = new ClrTypeInfo(typeof(TEntity)) };
            }
        }
    }
}