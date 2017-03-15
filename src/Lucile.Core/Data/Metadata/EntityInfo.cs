using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Lucile.Data.Metadata
{
    public class EntityInfo
    {
        private static MethodInfo _groupJoinMethod;
        private static MethodInfo _ofTypeMethod;
        private static MethodInfo _whereMethod;

        private readonly Func<IEnumerable<object>, IEnumerable<object>, IEnumerable<IJoinedResult<object>>> _mergeByKeyDelegate;

        static EntityInfo()
        {
            Expression<Func<IEnumerable, IEnumerable<object>>> ofTypeMethod = p => p.OfType<object>();
            Expression<Func<IEnumerable<object>, IEnumerable<object>, Func<object, object>, Func<object, object>, Func<object, IEnumerable<object>, object>, IEnumerable<object>>> groupJoinMethod = (a, b, c, d, e) => a.GroupJoin(b, c, d, e);
            Expression<Func<IEnumerable<object>, Func<object, bool>, IEnumerable<object>>> whereMethod = (p, q) => p.Where(q);
            EntityInfo._ofTypeMethod = ((MethodCallExpression)ofTypeMethod.Body).Method.GetGenericMethodDefinition();
            EntityInfo._groupJoinMethod = ((MethodCallExpression)groupJoinMethod.Body).Method.GetGenericMethodDefinition();
            EntityInfo._whereMethod = ((MethodCallExpression)whereMethod.Body).Method.GetGenericMethodDefinition();
        }

        public EntityInfo(MetadataModel model, EntityMetadata entity)
        {
            EntityMetadata = entity;

            // RootType ermitteln
            var root = entity;
            while (root.BaseEntity != null)
            {
                root = root.BaseEntity;
            }

            RootType = root;

            var scalarBuilder = ImmutableList.CreateBuilder<ScalarProperty>();
            var navigationBuilder = ImmutableList.CreateBuilder<NavigationPropertyMetadata>();
            var reverseBuilder = ImmutableList.CreateBuilder<NavigationPropertyMetadata>();
            var foreignKeyBuilder = ImmutableDictionary.CreateBuilder<ScalarProperty, NavigationPropertyMetadata>();

            // Properties befüllen
            FillProperties(model, entity, scalarBuilder, navigationBuilder, reverseBuilder, foreignKeyBuilder);

            NavigationProperties = navigationBuilder.ToImmutable();
            ReverseNavigationProperties = reverseBuilder.ToImmutable();
            ScalarProperties = scalarBuilder.ToImmutable();
            ForeignKeyProperties = foreignKeyBuilder.ToImmutable();

            _mergeByKeyDelegate = GetMergeByKeyDelegate(root);
        }

        private interface IJoinedResult<out T>
        {
            T Left { get; }

            IEnumerable<T> Right { get; }
        }

        public static MethodInfo GroupJoinMethod1
        {
            get
            {
                return _groupJoinMethod;
            }

            set
            {
                _groupJoinMethod = value;
            }
        }

        public EntityMetadata EntityMetadata { get; }

        public ImmutableDictionary<ScalarProperty, NavigationPropertyMetadata> ForeignKeyProperties { get; }

        public ImmutableList<NavigationPropertyMetadata> NavigationProperties { get; }

        public ImmutableList<NavigationPropertyMetadata> ReverseNavigationProperties { get; }

        public EntityMetadata RootType { get; }

        public ImmutableList<ScalarProperty> ScalarProperties { get; }

        public IEnumerable<object> GetMissingSourceObjects(IEnumerable<object> source, IEnumerable<object> target)
        {
            return _mergeByKeyDelegate(source, target)
                .Where(p => !p.Right.Any())
                .Select(p => p.Left);
        }

        private static void FillProperties(
            MetadataModel model,
            EntityMetadata entity,
            ImmutableList<ScalarProperty>.Builder scalarProperties,
            ImmutableList<NavigationPropertyMetadata>.Builder navigationProperties,
            ImmutableList<NavigationPropertyMetadata>.Builder reverseNavigationProperties,
            ImmutableDictionary<ScalarProperty, NavigationPropertyMetadata>.Builder foreignKeys)
        {
            // Skalar Properties
            foreach (var prop in entity.GetProperties())
            {
                scalarProperties.Add(prop);
            }

            // Navigation Properties
            foreach (var prop in entity.GetNavigations())
            {
                navigationProperties.Add(prop);
            }

            // Reverse-Navigation Properties inkl. aller Base-Entities ermitteln
            var parent = entity;
            while (parent != null)
            {
                foreach (var prop in model.Entities
                    .SelectMany(p => p.GetNavigations())
                    .Where(p => p.TargetEntity == parent)
                    .Distinct())
                {
                    reverseNavigationProperties.Add(prop);
                }

                parent = parent.BaseEntity;
            }

            // Foreign-Keys ermitteln
            var result = from np in navigationProperties
                         from fk in np.ForeignKeyProperties
                         select new { fk.Dependant, Nav = np };
            result.ToList().ForEach(p => foreignKeys.Add(p.Dependant, p.Nav));
        }

        private static Func<IEnumerable<object>, IEnumerable<object>, IEnumerable<IJoinedResult<object>>> GetMergeByKeyDelegate(EntityMetadata rootEntity)
        {
            var entityType = rootEntity.ClrType;
            var keyType = rootEntity.PrimaryKeyType;

            var param1 = Expression.Parameter(typeof(IEnumerable<object>));
            var param2 = Expression.Parameter(typeof(IEnumerable<object>));

            var cast1 = Expression.Call(_ofTypeMethod.MakeGenericMethod(entityType), param1);
            var cast2 = Expression.Call(_ofTypeMethod.MakeGenericMethod(entityType), param2);

            Expression keyBody = null;
            var keyParam = Expression.Parameter(entityType);
            var pks = rootEntity.GetProperties().Where(p => p.IsPrimaryKey).ToList();
            if (pks.Count > 1)
            {
                keyBody = Expression.MemberInit(
                    Expression.New(keyType),
                    pks.Select((p, i) =>
                        Expression.Bind(keyType.GetProperty($"Value{i}"), Expression.Property(keyParam, p.Name))));
            }
            else if (pks.Count == 1)
            {
                keyBody = Expression.Property(keyParam, pks.First().Name);
            }
            else
            {
                throw new NotSupportedException("Entities without primary key are not supported");
            }

            var keySelector = Expression.Lambda(keyBody, keyParam);

            var resultType = typeof(JoinedResult<>).MakeGenericType(entityType);

            var paramLeft = Expression.Parameter(entityType);
            var paramRight = Expression.Parameter(typeof(IEnumerable<>).MakeGenericType(entityType));
            var resultBody = Expression.New(resultType.GetConstructor(new[] { paramLeft.Type, paramRight.Type }), paramLeft, paramRight);
            var resultSelector = Expression.Lambda(resultBody, paramLeft, paramRight);

            var groupJoin = Expression.Call(
                _groupJoinMethod.MakeGenericMethod(entityType, entityType, keyType, resultType),
                cast1,
                cast2,
                keySelector,
                keySelector,
                resultSelector);

            var lambda = Expression.Lambda<Func<IEnumerable<object>, IEnumerable<object>, IEnumerable<IJoinedResult<object>>>>(groupJoin, param1, param2);
            return lambda.Compile();
        }

        private class JoinedResult<T> : IJoinedResult<T>
        {
            public JoinedResult(T left, IEnumerable<T> right)
            {
                Left = left;
                Right = right;
            }

            public T Left
            {
                get;
                private set;
            }

            public IEnumerable<T> Right
            {
                get;
                private set;
            }
        }
    }
}