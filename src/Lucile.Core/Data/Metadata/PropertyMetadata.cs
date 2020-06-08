using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using Lucile.Data.Metadata.Builder;

namespace Lucile.Data.Metadata
{
    public abstract class PropertyMetadata : MetadataElement
    {
        private static ConcurrentDictionary<Type, object> _defaultValueCache;

        private Func<object, object> _getValueDelegate;

        private Action<object, object> _setValueDelegate;

        static PropertyMetadata()
        {
            _defaultValueCache = new ConcurrentDictionary<Type, object>();
        }

        internal PropertyMetadata(EntityMetadata entity, IMetadataBuilder propBuilder)
            : base(propBuilder.Name)
        {
            var clrProperty = entity.ClrType.GetProperty(propBuilder.Name);

            Entity = entity;
            Nullable = propBuilder.Nullable;
            PropertyType = propBuilder.PropertyType?.ClrType ?? clrProperty?.PropertyType;
            Default = _defaultValueCache.GetOrAdd(PropertyType, p => CreateDefaultValue(p));

            HasClrProperty = clrProperty != null;
        }

        public object Default { get; }

        public EntityMetadata Entity { get; }

        public bool HasClrProperty { get; }

        public bool Nullable { get; }

        public Type PropertyType { get; }

        public object GetValue(object parameter)
        {
            if (_getValueDelegate == null)
            {
                _getValueDelegate = Entity.ValueAccessor.CreateGetValueDelegate(Name);
            }

            return this._getValueDelegate(parameter);
        }

        public void SetValue(object parameter, object value)
        {
            if (_setValueDelegate == null)
            {
                _setValueDelegate = Entity.ValueAccessor.CreateSetValueDelegate(Name);

                if (_setValueDelegate == null)
                {
                    throw new NotSupportedException($"The property {this.Name} on Entity {Entity} is not writable.");
                }
            }

            this._setValueDelegate(parameter, value);
        }

        private static object CreateDefaultValue(Type propertyType)
        {
            return Expression.Lambda<Func<object>>(
                            Expression.Convert(
                                Expression.Default(propertyType), typeof(object)))
                                .Compile()();
        }
    }
}