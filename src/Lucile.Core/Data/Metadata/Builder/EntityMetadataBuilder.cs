using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Lucile.Data.Metadata.Builder.Navigation;

namespace Lucile.Data.Metadata.Builder
{
    [DataContract(IsReference = true)]
    public class EntityMetadataBuilder
    {
        private ConcurrentDictionary<string, NavigationPropertyBuilder> _navigations;

        private List<string> _primaryKeys;
        private ConcurrentDictionary<string, ScalarPropertyBuilder> _properties;

        public EntityMetadataBuilder()
        {
        }

        public EntityMetadataBuilder(MetadataModelBuilder modelBuilder)
        {
            ModelBuilder = modelBuilder;
            _properties = new ConcurrentDictionary<string, ScalarPropertyBuilder>();
            _navigations = new ConcurrentDictionary<string, NavigationPropertyBuilder>();
        }

        [DataMember(Order = 2)]
        public EntityMetadataBuilder BaseEntity { get; set; }

        [DataMember(Order = 3)]
        public MetadataModelBuilder ModelBuilder { get; set; }

        public virtual string Name
        {
            get
            {
                return TypeInfo?.ClrType?.Name;
            }
        }

        [DataMember(Order = 5)]
        public IEnumerable<NavigationPropertyBuilder> Navigations
        {
            get
            {
                return _navigations.Values.ToList();
            }

            set
            {
                var values = value.ToDictionary(
                                        p => p.Name,
                                        p => p);

                _navigations = new ConcurrentDictionary<string, NavigationPropertyBuilder>(values);
            }
        }

        [DataMember(Order = 4)]
        public virtual List<string> PrimaryKey
        {
            get
            {
                if (_primaryKeys == null)
                {
                    _primaryKeys = new List<string>();
                }

                return _primaryKeys;
            }
        }

        [DataMember(Order = 6)]
        public IEnumerable<ScalarPropertyBuilder> Properties
        {
            get
            {
                return _properties.Values.ToList();
            }

            set
            {
                var values = value.ToDictionary(
                                        p => p.Name,
                                        p => p);

                _properties = new ConcurrentDictionary<string, ScalarPropertyBuilder>(values);
            }
        }

        [DataMember(Order = 1)]
        public virtual ClrTypeInfo TypeInfo { get; set; }

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
            return _navigations.GetOrAdd(propertyName, p => new NavigationPropertyBuilder { Name = propertyName });
        }

        public virtual ScalarPropertyBuilder Property(string name)
        {
            if (TypeInfo?.ClrType == null)
            {
                throw new InvalidOperationException($"The {nameof(TypeInfo)} property is not set.");
            }

            return _properties.GetOrAdd(name, CreatPropertyMetadata);
        }

        private ScalarPropertyBuilder CreatPropertyMetadata(string name)
        {
            var type = TypeInfo?.ClrType;

            if (type == null)
            {
                throw new InvalidOperationException($"The {nameof(TypeInfo)} property is not set.");
            }

            var propertyInfo = type.GetProperty(name);

            if (propertyInfo == null)
            {
                throw new ArgumentException($"Propertys {name} does not exist on Type {type}.", nameof(name));
            }

            return PropertyMetadataBuilder.CreateScalar(propertyInfo);
        }
    }
}