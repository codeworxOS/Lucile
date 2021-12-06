using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Lucile.Data.Metadata.Builder;

namespace Lucile.Data.Metadata
{
    public class NavigationPropertyMetadata : PropertyMetadata, INavigationProperty
    {
        private Action<object, object> _addItemDelegate;
        private Func<object, object, bool> _matchForeignKeyDelegate;
        private Action<object, object> _removeItemDelegate;

        private NavigationPropertyMetadata _targetNavigationProperty;

        internal NavigationPropertyMetadata(ModelCreationScope scope, EntityMetadata entity, NavigationPropertyBuilder builder)
                    : base(entity, builder)
        {
            scope.AddNavigationProperty(entity, builder.Name, this);

            this.Multiplicity = builder.Multiplicity;
            this.TargetMultiplicity = builder.TargetMultiplicity;
            this.TargetEntity = scope.GetEntity(builder.Target.ClrType);
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
                foreignKeyBuilder.Add(new ForeignKey(principalKeys[i], entity.GetProperties(true).First(p => p.Name == fk)));
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
                    if (_targetNavigationProperty == null)
                    {
                        _targetNavigationProperty = _targetNavigationProperty ?? (NavigationPropertyMetadata)TargetEntity[TargetNavigationPropertyName];
                    }

                    return _targetNavigationProperty;
                }

                return null;
            }
        }

        public string TargetNavigationPropertyName { get; }

        INavigationProperty INavigationProperty.TargetNavigationProperty => TargetNavigationProperty;

        IEntityMetadata INavigationProperty.TargetEntity => TargetEntity;

        IEnumerable<IForeignKey> INavigationProperty.ForeignKeyProperties => ForeignKeyProperties;

        public void AddItem(object parameter, object value)
        {
            if (this.Multiplicity != NavigationPropertyMultiplicity.Many)
            {
                throw new InvalidOperationException("AddItem may only be called for NavigationProperties with Multiplicity = Many.");
            }

            if (this._addItemDelegate == null)
            {
                _addItemDelegate = Entity.ValueAccessor.CreateAddItemDelegate(this);
            }

            this._addItemDelegate(parameter, value);
        }

        public object GetForeignKeyObject(object result)
        {
            if (Multiplicity != NavigationPropertyMultiplicity.Many && TargetMultiplicity != NavigationPropertyMultiplicity.Many)
            {
                return Entity.GetPrimaryKeyObject(result);
            }

            if (ForeignKeyProperties.IsEmpty || ForeignKeyProperties.Count > 1)
            {
                throw new NotImplementedException("Currently only single Primary Key objects are supported!");
            }

            return ForeignKeyProperties[0].Dependant.GetValue(result);
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
                _matchForeignKeyDelegate = Entity.ValueAccessor.CreatMatchForeignKeyDelegate(this);
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
                _removeItemDelegate = Entity.ValueAccessor.CreateRemoveItemDelegate(this);
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
                    if (target != null)
                    {
                        AddItem(entity, target);
                    }

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
    }
}