using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Lucile.Data;
using Lucile.Data.Metadata;

namespace Lucile.Core.Data.Metadata.Expressions
{
    public class ExpressionEntityValueAccessor : IEntityValueAccessor
    {
        private readonly Func<IEntityMetadata, EntityKey> _createEntityKeyDelegate;
        private readonly IEntityMetadata _entity;
        private readonly Func<object, bool> _isOfTypeDelegate;
        private readonly Func<object, object, bool> _keyEqualsDelegate;
        private readonly Type _keyType;
        private readonly List<IScalarProperty> _primaryKeys;

        public ExpressionEntityValueAccessor(IEntityMetadata entity)
        {
            _entity = entity;

            var param = Expression.Parameter(typeof(object), "p");

            var isOfTypeExpression = Expression.Lambda<Func<object, bool>>(
                        Expression.TypeIs(param, entity.ClrType),
                        param);

            _isOfTypeDelegate = isOfTypeExpression.Compile();

            _primaryKeys = entity.GetProperties().Where(p => p.IsPrimaryKey).ToList();

            if (_primaryKeys.Count == 1)
            {
                _keyType = _primaryKeys[0].PropertyType;
            }
            else if (_primaryKeys.Count > 1)
            {
                var keyTypes = _primaryKeys.Select(p => p.PropertyType).ToArray();
                _keyType = EntityKey.Get(keyTypes);
                _createEntityKeyDelegate = GetCreateEntityKeyDelegate(_keyType);
            }

            if (_primaryKeys.Count > 0)
            {
                this._keyEqualsDelegate = GetKeyEqualsDelegate(_entity.ClrType, _primaryKeys);
            }
        }

        public Action<object, object> CreateAddItemDelegate(INavigationProperty navigationProperty)
        {
            var lambda = GetCollectionOperationExpression(navigationProperty, "Add");
            return lambda.Compile();
        }

        public Func<object, object> CreateGetValueDelegate(string propertyName)
        {
            var propInfo = _entity.ClrType.GetProperty(propertyName);

            if (propInfo != null)
            {
                return GetGetValueDelegate(propInfo);
            }

            return null;
        }

        public Action<object, object> CreateRemoveItemDelegate(INavigationProperty navigationProperty)
        {
            var lambda = GetCollectionOperationExpression(navigationProperty, "Remove");
            return lambda.Compile();
        }

        public Action<object, object> CreateSetValueDelegate(string propertyName)
        {
            var propInfo = _entity.ClrType.GetProperty(propertyName);

            if (propInfo.CanWrite)
            {
                return GetSetValueDelegate(propInfo);
            }

            return null;
        }

        public Func<object, object, bool> CreatMatchForeignKeyDelegate(INavigationProperty navigationProperty)
        {
            Expression body = null;

            var parameterExpression = Expression.Parameter(typeof(object));
            var valueParameterExpression = Expression.Parameter(typeof(object));

            var paramConvert = Expression.Convert(parameterExpression, _entity.ClrType);
            var valueConvert = Expression.Convert(valueParameterExpression, navigationProperty.TargetEntity.ClrType);

            IDictionary<IScalarProperty, IScalarProperty> keyProperties = null;

            if (navigationProperty.Multiplicity == NavigationPropertyMultiplicity.One || navigationProperty.Multiplicity == NavigationPropertyMultiplicity.ZeroOrOne)
            {
                if (navigationProperty.TargetNavigationProperty != null && (navigationProperty.TargetNavigationProperty.Multiplicity == NavigationPropertyMultiplicity.One || navigationProperty.TargetNavigationProperty.Multiplicity == NavigationPropertyMultiplicity.ZeroOrOne))
                {
                    keyProperties = _primaryKeys
                                        .Where(p => p.IsPrimaryKey)
                                        .Select((p, i) => new { Prop = p, Index = i })
                                        .ToDictionary(
                                            p => p.Prop,
                                            p => navigationProperty.TargetEntity.GetProperties().Where(x => x.IsPrimaryKey).ElementAt(p.Index));
                }
            }

            if (keyProperties == null)
            {
                keyProperties = navigationProperty.ForeignKeyProperties.ToDictionary(p => p.Dependant, p => p.Principal);
            }

            foreach (var item in keyProperties)
            {
                var propLeft = Expression.Property(paramConvert, item.Key.Name);
                var propRight = Expression.Property(valueConvert, item.Value.Name);

                Expression expression = GetPropertyCompareExpression(propLeft, propRight);

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
            return lambda.Compile();
        }

        public object GetPrimaryKey(object entity)
        {
            if (_primaryKeys.Count == 1)
            {
                return _primaryKeys[0].GetValue(entity);
            }
            else if (_primaryKeys.Count > 1)
            {
                var key = _createEntityKeyDelegate(_entity);
                int i = 0;
                foreach (var item in _primaryKeys)
                {
                    key.SetValue(i, item.GetValue(entity));
                    i++;
                }

                return key;
            }

            return null;
        }

        public Type GetPrimaryKeyType() => _keyType;

        public bool IsOfType(object value)
        {
            return _isOfTypeDelegate(value);
        }

        public bool KeyEquals(object leftEntity, object rightEntity) => _keyEqualsDelegate(leftEntity, rightEntity);

        private static Func<IEntityMetadata, EntityKey> GetCreateEntityKeyDelegate(Type primaryKeyType)
        {
            var param = Expression.Parameter(typeof(IEntityMetadata));
            return Expression.Lambda<Func<IEntityMetadata, EntityKey>>(Expression.New(primaryKeyType.GetConstructor(new[] { typeof(IEntityMetadata) }), param), param).Compile();
        }

        private static Func<object, object> GetGetValueDelegate(PropertyInfo propInfo)
        {
            var parameterExpression = Expression.Parameter(typeof(object));
            var castExpression = Expression.Convert(parameterExpression, propInfo.DeclaringType);
            var propertyExpression = Expression.Property(castExpression, propInfo);
            var lambda = Expression.Lambda<Func<object, object>>(Expression.Convert(propertyExpression, typeof(object)), parameterExpression);
            return lambda.Compile();
        }

        private static Func<object, object, bool> GetKeyEqualsDelegate(Type clrType, IEnumerable<IScalarProperty> keyProperties)
        {
            Expression compareExpression = null;
            ParameterExpression paramLeft = Expression.Parameter(typeof(object));
            ParameterExpression paramRight = Expression.Parameter(typeof(object));

            foreach (var item in keyProperties)
            {
                var propLeft = Expression.Property(Expression.Convert(paramLeft, clrType), item.Name);
                var propRight = Expression.Property(Expression.Convert(paramRight, clrType), item.Name);

                Expression expression = null;

                expression = GetPropertyCompareExpression(propLeft, propRight);

                if (compareExpression == null)
                {
                    compareExpression = expression;
                }
                else
                {
                    compareExpression = Expression.And(compareExpression, expression);
                }
            }

            var lambda = Expression.Lambda<Func<object, object, bool>>(compareExpression, paramLeft, paramRight);
            return lambda.Compile();
        }

        private static Expression GetPropertyCompareExpression(MemberExpression propLeft, MemberExpression propRight)
        {
            Expression expression;
            if (propLeft.Type.GetTypeInfo().IsPrimitive)
            {
                expression = Expression.Equal(propLeft, propRight);
            }
            else
            {
                var equalsMethod = propLeft.Type.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                    .Where(p => p.Name == "Equals" && p.GetParameters().Count() == 1 && (p.GetParameters().First().ParameterType == typeof(object) || p.GetParameters().First().ParameterType == propLeft.Type))
                    .OrderBy(p => p.GetParameters().First().ParameterType == propLeft.Type ? 0 : 1)
                    .FirstOrDefault();

                expression = Expression.Call(
                                    propLeft,
                                    equalsMethod,
                                    equalsMethod.GetParameters().First().ParameterType == typeof(object) ? (Expression)Expression.Convert(propRight, typeof(object)) : propRight);
            }

            return expression;
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

        private Expression<Action<object, object>> GetCollectionOperationExpression(INavigationProperty navigationProperty, string methodName)
        {
            var parameterExpression = Expression.Parameter(typeof(object));
            var valueParameterExpression = Expression.Parameter(typeof(object));

            var lookupType = navigationProperty.TargetEntity.ClrType;

            var addExpression = typeof(ICollection<>)
                                    .MakeGenericType(lookupType)
                                    .GetMethod(methodName);

            var propertyExpression = Expression.Property(Expression.Convert(parameterExpression, _entity.ClrType), navigationProperty.Name);

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
