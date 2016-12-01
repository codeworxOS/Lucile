using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Lucile.Data.Metadata.Builder.Navigation;

namespace Lucile.Data.Metadata.Builder
{
    public class EntityMetadataBuilder<TEntity> : EntityMetadataBuilder
        where TEntity : class
    {
        private readonly EntityMetadataBuilder _innerBuilder;

        public EntityMetadataBuilder(EntityMetadataBuilder innerBuilder)
        {
            _innerBuilder = innerBuilder;
        }

        public override EntityMetadataBuilder BaseEntity
        {
            get
            {
                return _innerBuilder.BaseEntity;
            }

            set
            {
                _innerBuilder.BaseEntity = value;
            }
        }

        public override MetadataModelBuilder ModelBuilder
        {
            get
            {
                return _innerBuilder.ModelBuilder;
            }

            set
            {
                _innerBuilder.ModelBuilder = value;
            }
        }

        public override string Name
        {
            get
            {
                return _innerBuilder.Name;
            }
        }

        public override IEnumerable<NavigationPropertyBuilder> Navigations
        {
            get
            {
                return _innerBuilder.Navigations;
            }

            set
            {
                _innerBuilder.Navigations = value;
            }
        }

        public override List<string> PrimaryKey
        {
            get
            {
                return _innerBuilder.PrimaryKey;
            }
        }

        public override IEnumerable<ScalarPropertyBuilder> Properties
        {
            get
            {
                return _innerBuilder.Properties;
            }

            set
            {
                _innerBuilder.Properties = value;
            }
        }

        public override ClrTypeInfo TypeInfo
        {
            get
            {
                return _innerBuilder.TypeInfo;
            }

            set
            {
                _innerBuilder.TypeInfo = value;
            }
        }

        public ManyNavigationBuilder<TEntity, TTarget> HasMany<TTarget>(Expression<Func<TEntity, IEnumerable<TTarget>>> propertySelector)
            where TTarget : class
        {
            var propertyName = propertySelector.GetPropertyName();
            return new ManyNavigationBuilder<TEntity, TTarget>(_innerBuilder, propertyName);
        }

        public override ManyNavigationBuilder HasMany(Type targetType, string propertyName = null)
        {
            return _innerBuilder.HasMany(targetType, propertyName);
        }

        public OneNavigationBuilder<TEntity, TTarget> HasOne<TTarget>(Expression<Func<TEntity, TTarget>> propertySelector)
                            where TTarget : class
        {
            var propertyName = propertySelector.GetPropertyName();
            return new OneNavigationBuilder<TEntity, TTarget>(_innerBuilder, propertyName);
        }

        public override OneNavigationBuilder HasOne(Type targetType, string propertyName = null)
        {
            return _innerBuilder.HasOne(targetType, propertyName);
        }

        public override NavigationPropertyBuilder Navigation(string propertyName)
        {
            return _innerBuilder.Navigation(propertyName);
        }

        public override ScalarPropertyBuilder Property(string name)
        {
            return _innerBuilder.Property(name);
        }

        public TextPropertyBuilder Property(Expression<Func<TEntity, string>> propertySelector)
        {
            var propertyName = propertySelector.GetPropertyName();
            return (TextPropertyBuilder)_innerBuilder.Property(propertyName);
        }

        public GuidPropertyBuilder Property(Expression<Func<TEntity, Guid>> propertySelector)
        {
            var propertyName = propertySelector.GetPropertyName();
            return (GuidPropertyBuilder)_innerBuilder.Property(propertyName);
        }

        public GuidPropertyBuilder Property(Expression<Func<TEntity, Guid?>> propertySelector)
        {
            var propertyName = propertySelector.GetPropertyName();
            return (GuidPropertyBuilder)_innerBuilder.Property(propertyName);
        }

        public NumericPropertyBuilder Property(Expression<Func<TEntity, byte>> propertySelector)
        {
            return NumericProperty(propertySelector.GetPropertyName());
        }

        public NumericPropertyBuilder Property(Expression<Func<TEntity, sbyte>> propertySelector)
        {
            return NumericProperty(propertySelector.GetPropertyName());
        }

        public NumericPropertyBuilder Property(Expression<Func<TEntity, short>> propertySelector)
        {
            return NumericProperty(propertySelector.GetPropertyName());
        }

        public NumericPropertyBuilder Property(Expression<Func<TEntity, ushort>> propertySelector)
        {
            return NumericProperty(propertySelector.GetPropertyName());
        }

        public NumericPropertyBuilder Property(Expression<Func<TEntity, int>> propertySelector)
        {
            return NumericProperty(propertySelector.GetPropertyName());
        }

        public NumericPropertyBuilder Property(Expression<Func<TEntity, uint>> propertySelector)
        {
            return NumericProperty(propertySelector.GetPropertyName());
        }

        public NumericPropertyBuilder Property(Expression<Func<TEntity, long>> propertySelector)
        {
            return NumericProperty(propertySelector.GetPropertyName());
        }

        public NumericPropertyBuilder Property(Expression<Func<TEntity, ulong>> propertySelector)
        {
            return NumericProperty(propertySelector.GetPropertyName());
        }

        public NumericPropertyBuilder Property(Expression<Func<TEntity, float>> propertySelector)
        {
            return NumericProperty(propertySelector.GetPropertyName());
        }

        public NumericPropertyBuilder Property(Expression<Func<TEntity, double>> propertySelector)
        {
            return NumericProperty(propertySelector.GetPropertyName());
        }

        public NumericPropertyBuilder Property(Expression<Func<TEntity, decimal>> propertySelector)
        {
            return NumericProperty(propertySelector.GetPropertyName());
        }

        public NumericPropertyBuilder Property(Expression<Func<TEntity, byte?>> propertySelector)
        {
            return NumericProperty(propertySelector.GetPropertyName());
        }

        public NumericPropertyBuilder Property(Expression<Func<TEntity, sbyte?>> propertySelector)
        {
            return NumericProperty(propertySelector.GetPropertyName());
        }

        public NumericPropertyBuilder Property(Expression<Func<TEntity, short?>> propertySelector)
        {
            return NumericProperty(propertySelector.GetPropertyName());
        }

        public NumericPropertyBuilder Property(Expression<Func<TEntity, ushort?>> propertySelector)
        {
            return NumericProperty(propertySelector.GetPropertyName());
        }

        public NumericPropertyBuilder Property(Expression<Func<TEntity, int?>> propertySelector)
        {
            return NumericProperty(propertySelector.GetPropertyName());
        }

        public NumericPropertyBuilder Property(Expression<Func<TEntity, uint?>> propertySelector)
        {
            return NumericProperty(propertySelector.GetPropertyName());
        }

        public NumericPropertyBuilder Property(Expression<Func<TEntity, long?>> propertySelector)
        {
            return NumericProperty(propertySelector.GetPropertyName());
        }

        public NumericPropertyBuilder Property(Expression<Func<TEntity, ulong?>> propertySelector)
        {
            return NumericProperty(propertySelector.GetPropertyName());
        }

        public NumericPropertyBuilder Property(Expression<Func<TEntity, float?>> propertySelector)
        {
            return NumericProperty(propertySelector.GetPropertyName());
        }

        public NumericPropertyBuilder Property(Expression<Func<TEntity, double?>> propertySelector)
        {
            return NumericProperty(propertySelector.GetPropertyName());
        }

        public NumericPropertyBuilder Property(Expression<Func<TEntity, decimal?>> propertySelector)
        {
            return NumericProperty(propertySelector.GetPropertyName());
        }

        public DateTimePropertyBuilder Property(Expression<Func<TEntity, DateTime>> propertySelector)
        {
            return (DateTimePropertyBuilder)_innerBuilder.Property(propertySelector.GetPropertyName());
        }

        public DateTimePropertyBuilder Property(Expression<Func<TEntity, DateTime?>> propertySelector)
        {
            return (DateTimePropertyBuilder)_innerBuilder.Property(propertySelector.GetPropertyName());
        }

        public DateTimePropertyBuilder Property(Expression<Func<TEntity, DateTimeOffset>> propertySelector)
        {
            return (DateTimePropertyBuilder)_innerBuilder.Property(propertySelector.GetPropertyName());
        }

        public DateTimePropertyBuilder Property(Expression<Func<TEntity, DateTimeOffset?>> propertySelector)
        {
            return (DateTimePropertyBuilder)_innerBuilder.Property(propertySelector.GetPropertyName());
        }

        public BooleanPropertyBuilder Property(Expression<Func<TEntity, bool>> propertySelector)
        {
            return (BooleanPropertyBuilder)_innerBuilder.Property(propertySelector.GetPropertyName());
        }

        public BooleanPropertyBuilder Property(Expression<Func<TEntity, bool?>> propertySelector)
        {
            return (BooleanPropertyBuilder)_innerBuilder.Property(propertySelector.GetPropertyName());
        }

        public EnumPropertyBuilder Property<TEnum>(Expression<Func<TEntity, TEnum>> propertySelector)
             where TEnum : struct
        {
            if (!typeof(TEnum).GetTypeInfo().IsEnum)
            {
                throw new InvalidOperationException($"Type {nameof(TEnum)} is not an Enum type.");
            }

            return (EnumPropertyBuilder)_innerBuilder.Property(propertySelector.GetPropertyName());
        }

        public EnumPropertyBuilder Property<TEnum>(Expression<Func<TEntity, TEnum?>> propertySelector)
            where TEnum : struct
        {
            if (!typeof(TEnum).GetTypeInfo().IsEnum)
            {
                throw new InvalidOperationException($"Type {nameof(TEnum)} is not an Enum type.");
            }

            return (EnumPropertyBuilder)_innerBuilder.Property(propertySelector.GetPropertyName());
        }

        public BlobPropertyBuilder Property(Expression<Func<TEntity, byte[]>> propertySelector)
        {
            return (BlobPropertyBuilder)_innerBuilder.Property(propertySelector.GetPropertyName());
        }

        public EnumPropertyBuilder Property(Expression<Func<TEntity, Enum>> propertySelector)
        {
            return (EnumPropertyBuilder)_innerBuilder.Property(propertySelector.GetPropertyName());
        }

        protected NumericPropertyBuilder NumericProperty(string propertyName)
        {
            return (NumericPropertyBuilder)_innerBuilder.Property(propertyName);
        }
    }
}