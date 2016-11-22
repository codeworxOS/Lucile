using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Lucile.Data.Metadata.Builder;

namespace Lucile.Data.Metadata
{
    public class NavigationPropertyMetadata : PropertyMetadata
    {
        private Action<object, object> _addItemDelegate;
        private Func<object, object, bool> _matchForeignKeyDelegate;
        private Action<object, object> _removeItemDelegate;

        internal NavigationPropertyMetadata(ModelCreationScope scope, EntityMetadata entity, NavigationPropertyBuilder builder)
            : base(entity, builder)
        {
            scope.AddNavigationProperty(entity, builder.Name, this);

            this.Multiplicity = builder.Multiplicity.Value;
            this.TargetMultiplicity = builder.TargetMultiplicity.Value;
            this.TargetEntity = scope.GetEntity(builder.Target.ClrType);
            if (builder.TargetProperty != null)
            {
                this.TargetNavigationPropertyName = builder.TargetProperty;
                ////this.TargetNavigationProperty = scope.NavigationProperties[this.TargetEntity][builder.TargetProperty];
            }

            var foreignKeyBuilder = ImmutableList.CreateBuilder<ForeignKey>();

            var principalKeys = this.TargetEntity.GetProperties().Where(p => p.IsPrimaryKey).ToList();
            for (int i = 0; i < builder.ForeignKey.Count; i++)
            {
                var fk = builder.ForeignKey[i];
                foreignKeyBuilder.Add(new ForeignKey(principalKeys[i], entity.GetProperties().First(p => p.Name == fk)));
            }

            ForeignKeyProperties = foreignKeyBuilder.ToImmutable();
        }

        public ImmutableList<ForeignKey> ForeignKeyProperties { get; }

        public NavigationPropertyMultiplicity Multiplicity { get; }

        public EntityMetadata TargetEntity { get; }

        public NavigationPropertyMultiplicity TargetMultiplicity { get; }

        public NavigationPropertyMetadata TargetNavigationProperty
        {
            get
            {
                if (TargetNavigationPropertyName != null)
                {
                    return (NavigationPropertyMetadata)TargetEntity[TargetNavigationPropertyName];
                }

                return null;
            }
        }

        public string TargetNavigationPropertyName { get; }

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

        public object GetForeignKeyObject(object result)
        {
            if (!ForeignKeyProperties.Any() || ForeignKeyProperties.Count > 1)
            {
                throw new NotImplementedException("Currently only single Primary Key objects are supported!");
            }

            return ForeignKeyProperties.First().Dependant.GetValue(result);
        }

        public IEnumerable<object> GetItems(object entity)
        {
            var current = GetValue(entity);
            if (Multiplicity == NavigationPropertyMultiplicity.Many)
            {
                return (IEnumerable<object>)current ?? Enumerable.Empty<object>();
            }

            return current != null ? new[] { current } : Enumerable.Empty<object>();
        }

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
                        keyProperties = this.Entity.GetProperties()
                                            .Where(p => p.IsPrimaryKey)
                                            .Select((p, i) => new { Prop = p, Index = i })
                                            .ToDictionary(
                                                p => p.Prop,
                                                p => this.TargetEntity.GetProperties().Where(x => x.IsPrimaryKey).ElementAt(p.Index));
                    }
                }

                if (keyProperties == null)
                {
                    keyProperties = this.ForeignKeyProperties.ToDictionary(p => p.Dependant, p => p.Principal);
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

        public bool ReplaceItem(object entity, object source, object target)
        {
            var current = GetValue(entity);
            if (this.Multiplicity == NavigationPropertyMultiplicity.Many)
            {
                if (((IEnumerable<object>)current).Contains(source))
                {
                    RemoveItem(entity, source);
                    AddItem(entity, target);
                    return true;
                }
            }
            else
            {
                if (object.Equals(current, source))
                {
                    SetValue(entity, target);
                    return true;
                }
            }

            return false;
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