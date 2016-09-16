using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;

namespace Lucile.Data.Metadata
{
    /// <summary>
    /// Funktionalität Metadaten für ein Navigation-Property
    /// </summary>
    [DataContract(IsReference = true)]
    [ProtoBuf.ProtoContract(AsReferenceDefault = true)]
    public class NavigationPropertyMetadata : PropertyMetadata
    {
        private Action<object, object> _addItemDelegate;
        private Func<object, object, bool> _matchForeignKeyDelegate;
        private Action<object, object> _removeItemDelegate;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="enity"></param>
        public NavigationPropertyMetadata(EntityMetadata enity)
            : base(enity)
        {
            this.ForeignKeyProperties = new Dictionary<ScalarProperty, ScalarProperty>();
        }

        internal NavigationPropertyMetadata()
        {
        }

        /// <summary>
        /// Liefert doe Fremdschlüssel-Properties
        /// </summary>
        [DataMember(Order = 2)]
        public IDictionary<ScalarProperty, ScalarProperty> ForeignKeyProperties { get; private set; }

        /// <summary>
        /// Liefert oder setzt die Multiplicity
        /// </summary>
        [DataMember(Order = 1)]
        public NavigationPropertyMultiplicity Multiplicity { get; set; }

        /// <summary>
        /// Liefert oder setzt die Ziel-Entität
        /// </summary>
        [DataMember(Order = 4)]
        public EntityMetadata TargetEntity { get; set; }

        /// <summary>
        /// Liefert oder setzt das Ziel-Navigation-Property
        /// </summary>
        [DataMember(Order = 3)]
        public NavigationPropertyMetadata TargetNavigationProperty { get; set; }

        /// <summary>
        /// Fügt ein Item für ein Navigation-Property vom Typ Many hinzu
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="value"></param>
        public void AddItem(object parameter, object value)
        {
            if (this.Multiplicity != NavigationPropertyMultiplicity.Many)
            {
                throw new InvalidOperationException("AddItem may only be called for NavigationProperties with Multiplicity = Many.");
            }

            if (this._addItemDelegate == null)
            {
                var lambda = GetCollectionOperationExpression("Add");
                this._addItemDelegate = lambda.Compile();
            }

            this._addItemDelegate(parameter, value);
        }

        /// <summary>
        /// Liefert, ob die Fremdschlüssel-Keys passen
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="navigationValue"></param>
        /// <returns></returns>
        public bool MatchForeignKeys(object parameter, object navigationValue)
        {
            if (Multiplicity == NavigationPropertyMultiplicity.Many)
            {
                throw new InvalidOperationException("MatchForeignKeys may only be calld for NavigationProperties with Multiplicity = Many.");
            }

            if (this._matchForeignKeyDelegate == null)
            {
                Expression body = null;

                var parameterExpression = Expression.Parameter(typeof(object));
                var valueParameterExpression = Expression.Parameter(typeof(object));

                var paramConvert = Expression.Convert(parameterExpression, this.Entity.ClrType);
                var valueConvert = Expression.Convert(valueParameterExpression, this.TargetEntity.ClrType);

                IDictionary<ScalarProperty, ScalarProperty> keyProperties = null;

                if (Multiplicity == NavigationPropertyMultiplicity.One || Multiplicity == NavigationPropertyMultiplicity.ZeroOrOne)
                {
                    if (this.TargetNavigationProperty != null && (TargetNavigationProperty.Multiplicity == NavigationPropertyMultiplicity.One || TargetNavigationProperty.Multiplicity == NavigationPropertyMultiplicity.ZeroOrOne))
                    {
                        keyProperties = this.Entity.Properties.OfType<ScalarProperty>()
                                            .Where(p => p.IsPrimaryKey)
                                            .Select((p, i) => new { Prop = p, Index = i })
                                            .ToDictionary(
                                                p => p.Prop,
                                                p => this.TargetEntity.Properties.OfType<ScalarProperty>().Where(x => x.IsPrimaryKey).ElementAt(p.Index));
                    }
                }

                if (keyProperties == null)
                {
                    keyProperties = this.ForeignKeyProperties;
                }

                foreach (var item in keyProperties)
                {
                    var propLeft = Expression.Property(paramConvert, item.Key.Name);
                    var propRight = Expression.Property(valueConvert, item.Value.Name);

                    Expression expression = EntityMetadata.GetPropertyCompareExpression(propLeft, propRight);

                    if (body == null)
                    {
                        body = expression;
                    }
                    else
                    {
                        body = Expression.And(body, expression);
                    }
                }

                var lambda = Expression.Lambda<Func<object, object, bool>>(body, parameterExpression, valueParameterExpression);
                this._matchForeignKeyDelegate = lambda.Compile();
            }

            return this._matchForeignKeyDelegate(parameter, navigationValue);
        }

        /// <summary>
        /// Entfernt ein Item für ein Navigation-Property vom Typ Many
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="value"></param>
        public void RemoveItem(object parameter, object value)
        {
            if (this.Multiplicity != NavigationPropertyMultiplicity.Many)
            {
                throw new InvalidOperationException("AddItem may only be calld for NavigationProperties with Multiplicity = Many.");
            }

            if (this._removeItemDelegate == null)
            {
                var lambda = GetCollectionOperationExpression("Remove");
                this._removeItemDelegate = lambda.Compile();
            }

            this._removeItemDelegate(parameter, value);
        }

        private Expression<Action<object, object>> GetCollectionOperationExpression(string methodName)
        {
            var parameterExpression = Expression.Parameter(typeof(object));
            var valueParameterExpression = Expression.Parameter(typeof(object));

            var lookupType = TargetEntity.ClrType;

            var addExpression = typeof(ICollection<>)
                                    .MakeGenericType(lookupType)
                                    .GetMethod(methodName);

            var propertyExpression = Expression.Property(Expression.Convert(parameterExpression, Entity.ClrType), this.Name);

            Expression body = Expression.Call(propertyExpression, addExpression, Expression.Convert(valueParameterExpression, lookupType));

            if (methodName == "Add")
            {
                var contains = typeof(ICollection<>)
                                    .MakeGenericType(lookupType)
                                    .GetMethod("Contains");

                body = Expression.Condition(Expression.Call(propertyExpression, contains, Expression.Convert(valueParameterExpression, lookupType)), Expression.Empty(), body);
            }

            var lambda = Expression.Lambda<Action<object, object>>(body, parameterExpression, valueParameterExpression);
            return lambda;
        }
    }
}