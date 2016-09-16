using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;

namespace Lucile.Data.Metadata
{
    /// <summary>
    /// Funktionalität für ein Property-Metadata-Element
    /// </summary>
    [DataContract(IsReference = true)]
    [ProtoBuf.ProtoContract(AsReferenceDefault = true)]
    [KnownType(typeof(ScalarProperty))]
    [KnownType(typeof(NavigationPropertyMetadata))]
    [ProtoBuf.ProtoInclude(101, typeof(ScalarProperty))]
    [ProtoBuf.ProtoInclude(102, typeof(NavigationPropertyMetadata))]
    public abstract class PropertyMetadata : MetadataElement
    {
        private Func<object> _defaultValueDelegate;

        private Func<object, object> _getValueDelegate;

        private Action<object, object> _setValueDelegate;

        protected PropertyMetadata()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="entity"></param>
        protected PropertyMetadata(EntityMetadata entity)
        {
            this.Entity = entity;
        }

        public object DefaultValue
        {
            get
            {
                if (_defaultValueDelegate == null)
                {
                    _defaultValueDelegate = Expression.Lambda<Func<object>>(
                        Expression.Convert(
                            Expression.Default(this.Entity.ClrType.GetProperty(Name).PropertyType), typeof(object)))
                            .Compile();
                }

                return _defaultValueDelegate();
            }
        }

        /// <summary>
        /// Liefert die Entität
        /// </summary>
        [DataMember(Order = 1)]
        public EntityMetadata Entity { get; private set; }

        /// <summary>
        /// Liefert oder setzt, ob das Property nullable ist
        /// </summary>
        [DataMember(Order = 2)]
        public bool Nullable { get; set; }

        /// <summary>
        /// Liefert den Wert des Propertys
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public object GetValue(object parameter)
        {
            if (this._getValueDelegate == null)
            {
                var parameterExpression = Expression.Parameter(typeof(object));
                var castExpression = Expression.Convert(parameterExpression, Entity.ClrType);
                var propertyExpression = Expression.Property(castExpression, this.Name);
                var lambda = Expression.Lambda<Func<object, object>>(Expression.Convert(propertyExpression, typeof(object)), parameterExpression);
                this._getValueDelegate = lambda.Compile();
            }

            return this._getValueDelegate(parameter);
        }

        /// <summary>
        /// Setzt den Wert des Propertys
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="value"></param>
        public void SetValue(object parameter, object value)
        {
            if (this._setValueDelegate == null)
            {
                var parameterExpression = Expression.Parameter(typeof(object));
                var valueParameterExpression = Expression.Parameter(typeof(object));
                Expression castExpression = Expression.Convert(parameterExpression, Entity.ClrType);
                Expression propertyExpression = Expression.Property(castExpression, this.Name);
                castExpression = Expression.Convert(valueParameterExpression, propertyExpression.Type);
                if (this.Nullable && propertyExpression.Type.GetTypeInfo().IsValueType)
                {
                    var propType = propertyExpression.Type;
                    propType = System.Nullable.GetUnderlyingType(propType) ?? propType;
                    castExpression = Expression.Condition(
                        Expression.Equal(valueParameterExpression, Expression.Default(typeof(object))),
                            Expression.Default(propertyExpression.Type),
                            Expression.Convert(Expression.Convert(valueParameterExpression, propType), propertyExpression.Type));
                }

                var body = Expression.Assign(propertyExpression, castExpression);
                var lambda = Expression.Lambda<Action<object, object>>(body, parameterExpression, valueParameterExpression);
                this._setValueDelegate = lambda.Compile();
            }

            this._setValueDelegate(parameter, value);
        }
    }
}