﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Lucile.Data.Metadata.Builder.Navigation;

namespace Lucile.Data.Metadata.Builder
{
    [DataContract(IsReference = true)]
    [ProtoBuf.ProtoContract(AsReferenceDefault = true)]
    public class EntityMetadataBuilder
    {
        private MetadataModelBuilder _modelBuilder;
        private ICollection<NavigationPropertyBuilder> _navigations;
        private ICollection<string> _primaryKeys;
        private ICollection<ScalarPropertyBuilder> _properties;
        private object _propertiesLocker = new object();

        public EntityMetadataBuilder()
        {
            _navigations = new HashSet<NavigationPropertyBuilder>();
            _properties = new HashSet<ScalarPropertyBuilder>();
            _propertiesLocker = new object();
        }

        public EntityMetadataBuilder(MetadataModelBuilder modelBuilder)
                : this()
        {
            _modelBuilder = modelBuilder;
        }

        [DataMember(Order = 2)]
        public virtual EntityMetadataBuilder BaseEntity { get; set; }

        [DataMember(Order = 7)]
        public virtual bool IsExcluded { get; set; }

        [DataMember(Order = 3)]
        public virtual MetadataModelBuilder ModelBuilder
        {
            get { return _modelBuilder; }
            set { _modelBuilder = value; }
        }

        public virtual string Name
        {
            get
            {
                return TypeInfo?.GetFriendlyName();
            }
        }

        [DataMember(Order = 5)]
        public virtual ICollection<NavigationPropertyBuilder> Navigations
        {
            get
            {
                _navigations = _navigations ?? new HashSet<NavigationPropertyBuilder>();
                return _navigations;
            }
        }

        [DataMember(Order = 4)]
        public virtual ICollection<string> PrimaryKey
        {
            get
            {
                _primaryKeys = _primaryKeys ?? new HashSet<string>();

                return _primaryKeys;
            }
        }

        [DataMember(Order = 6)]
        public virtual ICollection<ScalarPropertyBuilder> Properties
        {
            get
            {
                _properties = _properties ?? new HashSet<ScalarPropertyBuilder>();
                return _properties;
            }
        }

        [DataMember(Order = 1)]
        public virtual ClrTypeInfo TypeInfo { get; set; }

        public EntityMetadataBuilder CopyFrom(EntityMetadataBuilder source)
        {
            if (source.TypeInfo.ClrType != TypeInfo.ClrType)
            {
                throw new NotSupportedException("The provided Source MetadataBuilder is for a differenct entity");
            }

            foreach (var item in source.Properties)
            {
                var targetProperty = this.Property(item.Name, item.PropertyType.ClrType);
                targetProperty.CopyFrom(item);
            }

            foreach (var item in source.PrimaryKey)
            {
                this.PrimaryKey.Add(item);
            }

            foreach (var item in source.Navigations)
            {
                var targetNavigation = this.Navigation(item.Name);
                targetNavigation.CopyFrom(item);
            }

            if (source.BaseEntity != null)
            {
                BaseEntity = ModelBuilder.Entity(source.BaseEntity.TypeInfo.ClrType);
            }

            return this;
        }

        public ManyNavigationBuilder HasMany<TTarget>(string propertyName = null)
        {
            return HasMany(typeof(TTarget), propertyName);
        }

        public virtual ManyNavigationBuilder HasMany(Type targetType, string propertyName = null)
        {
            return new ManyNavigationBuilder(this, targetType, propertyName);
        }

        public OneNavigationBuilder HasOne<TTarget>(string propertyName = null)
        {
            return HasOne(typeof(TTarget), propertyName);
        }

        public virtual OneNavigationBuilder HasOne(Type targetType, string propertyName = null)
        {
            return new OneNavigationBuilder(this, targetType, propertyName);
        }

        public virtual NavigationPropertyBuilder Navigation(string propertyName)
        {
            if (TypeInfo?.ClrType == null)
            {
                throw new InvalidOperationException($"The {nameof(TypeInfo)} property is not set.");
            }

            _propertiesLocker = _propertiesLocker ?? new object();

            lock (_propertiesLocker)
            {
                _navigations = _navigations ?? new HashSet<NavigationPropertyBuilder>();

                var result = _navigations.FirstOrDefault(p => p.Name == propertyName);
                if (result == null)
                {
                    result = CreateNavigationBuilder(propertyName);
                    _navigations.Add(result);
                }

                return result;
            }
        }

        public virtual ScalarPropertyBuilder Property<TValue>(string name)
        {
            return Property(name, typeof(TValue));
        }

        public virtual ScalarPropertyBuilder Property(string name, Type clrType)
        {
            if (TypeInfo?.ClrType == null)
            {
                throw new InvalidOperationException($"The {nameof(TypeInfo)} property is not set.");
            }

            _propertiesLocker = _propertiesLocker ?? new object();

            lock (_propertiesLocker)
            {
                _properties = _properties ?? new HashSet<ScalarPropertyBuilder>();

                var result = _properties.FirstOrDefault(p => p.Name == name);
                if (result == null)
                {
                    result = CreatePropertyMetadata(name, clrType);
                    _properties.Add(result);
                }

                return result;
            }
        }

        private NavigationPropertyBuilder CreateNavigationBuilder(string propertyName)
        {
            var type = TypeInfo.ClrType;

            var propertyInfo = type.GetProperty(propertyName);

            ////if (propertyInfo != null && propertyInfo.DeclaringType != this.TypeInfo.ClrType)
            ////{
            ////    var entity = this.ModelBuilder.Entity(propertyInfo.DeclaringType);
            ////    return entity.Navigation(propertyName);
            ////}

            if (propertyInfo == null)
            {
                throw new ArgumentException($"Propertys {propertyName} does not exist on Type {type}.", nameof(propertyName));
            }

            Type elementType;
            var isCollectionType = propertyInfo.PropertyType.IsCollectionType(out elementType);

            return new NavigationPropertyBuilder
            {
                Name = propertyName,
                Multiplicity = isCollectionType ? NavigationPropertyMultiplicity.Many : NavigationPropertyMultiplicity.ZeroOrOne,
                TargetMultiplicity = isCollectionType ? NavigationPropertyMultiplicity.ZeroOrOne : NavigationPropertyMultiplicity.Many,
                Target = new ClrTypeInfo(elementType),
            };
        }

        private ScalarPropertyBuilder CreatePropertyMetadata(string name, Type clrType)
        {
            return ScalarPropertyBuilder.CreateScalar(name, clrType);
        }
    }
}