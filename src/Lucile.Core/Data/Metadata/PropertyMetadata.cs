using System;
using System.Linq.Expressions;
using System.Reflection;
using Lucile.Data.Metadata.Builder;

namespace Lucile.Data.Metadata
{
    public abstract class PropertyMetadata : MetadataElement
    {
        private readonly Func<object> _defaultValueDelegate;

        private readonly Func<object, object> _getValueDelegate;

        private readonly Action<object, object> _setValueDelegate;

        internal PropertyMetadata(EntityMetadata entity, IMetadataBuilder propBuilder)
            : base(propBuilder.Name)
        {
            this.Entity = entity;
            this.Nullable = propBuilder.Nullable;
            var propInfo = this.Entity.ClrType.GetProperty(Name);

            HasClrProperty = propInfo != null;

            PropertyType = propInfo?.PropertyType ?? propBuilder.PropertyType.ClrType;
            _defaultValueDelegate = GetDefaultValueDelegate(PropertyType);

            if (propInfo != null)
            {
                this._getValueDelegate = GetGetValueDelegate(propInfo);

                if (propInfo.CanWrite)
                {
                    this._setValueDelegate = GetSetValueDelegate(propInfo);
                }
            }
        }

        public object Default
        {
            get
            {
                return _defaultValueDelegate();
            }
        }

        public EntityMetadata Entity { get; }

        public bool Nullable { get; }

        public bool HasClrProperty { get; }

        public Type PropertyType { get; }

        public object GetValue(object parameter)
        {
            return this._getValueDelegate(parameter);
        }

        public void SetValue(object parameter, object value)
        {
            if (_setValueDelegate == null)
            {
                throw new NotSupportedException($"The property {this.Name} on Entity {Entity} is not writable.");
            }

            this._setValueDelegate(parameter, value);
        }

        private static Func<object> GetDefaultValueDelegate(Type propertyType)
        {
            return Expression.Lambda<Func<object>>(
                            Expression.Convert(
                                Expression.Default(propertyType), typeof(object)))
                                .Compile();
        }

        private static Func<object, object> GetGetValueDelegate(PropertyInfo propInfo)
        {
            var parameterExpression = Expression.Parameter(typeof(object));
            var castExpression = Expression.Convert(parameterExpression, propInfo.DeclaringType);
            var propertyExpression = Expression.Property(castExpression, propInfo);
            var lambda = Expression.Lambda<Func<object, object>>(Expression.Convert(propertyExpression, typeof(object)), parameterExpression);
            return lambda.Compile();
        }

        private static Action<object, object> GetSetValueDelegate(PropertyInfo propInfo)
        {
            var nullableBase = System.Nullable.GetUnderlyingType(propInfo.PropertyType);

            var parameterExpression = Expression.Parameter(typeof(object));
            var castExpression = Expression.Convert(parameterExpression, propInfo.DeclaringType);
            var propertyExpression = Expression.Property(castExpression, propInfo);
            var valueParameterExpression = Expression.Parameter(typeof(object));
            Expression valuecastExpression = Expression.Convert(valueParameterExpression, propertyExpression.Type);
            if (nullableBase != null && propInfo.PropertyType.GetTypeInfo().IsValueType)
            {
                var propType = propertyExpression.Type;
                propType = System.Nullable.GetUnderlyingType(propType) ?? propType;
                valuecastExpression = Expression.Condition(
                    Expression.Equal(valueParameterExpression, Expression.Default(typeof(object))),
                    Expression.Default(propertyExpression.Type),
                    Expression.Convert(Expression.Convert(valueParameterExpression, propType), propertyExpression.Type));
            }

            var body = Expression.Assign(propertyExpression, valuecastExpression);
            var setLambda = Expression.Lambda<Action<object, object>>(body, parameterExpression, valueParameterExpression);

            return setLambda.Compile();
        }
    }
}