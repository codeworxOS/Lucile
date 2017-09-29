using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Lucile.Data.Metadata;
using Lucile.Linq.Configuration;
using Lucile.Reflection;

namespace Lucile.Linq
{
    public abstract class QueryModel : IQueryModel
    {
        public QueryModel(EntityMetadata entity, IEnumerable<SourceEntityConfiguration> sourceEntityConfigs, IEnumerable<PropertyConfiguration> propConfigs)
        {
            Entity = entity;
            SourceEntityConfigurations = ImmutableHashSet.Create<SourceEntityConfiguration>(sourceEntityConfigs.ToArray());
            PropertyConfigurations = ImmutableHashSet.Create<PropertyConfiguration>(propConfigs.ToArray());
        }

        public EntityMetadata Entity { get; }

        public ImmutableHashSet<PropertyConfiguration> PropertyConfigurations { get; }

        public abstract Type ResultType { get; }

        public ImmutableHashSet<SourceEntityConfiguration> SourceEntityConfigurations { get; }

        public abstract Type SourceType { get; }

        internal static object GlobalSourceKey { get; } = new object();

        public static QueryModelBuilder<TSource, TModel> Build<TSource, TModel>(Expression<Func<QuerySourceBuilder, TSource>> sourceSelector, Expression<Func<TSource, TModel>> queryExpression)
            where TModel : class
            where TSource : class
        {
            var builder = new QueryModelBuilder<TSource, TModel>(sourceSelector);
            builder.Map(queryExpression);
            return builder;
        }

        public virtual IQueryable GetQuery(QuerySource source, QueryConfiguration config)
        {
            var propsToQuery = PropertyConfigurations.ToList();

            if (config.Select.Any())
            {
                var columns = config.Select.ToList();

                var filterPaths = config.TargetFilterItems.SelectMany(p => p.GetValueExpressions()).OfType<PathValueExpression>().Select(p => p.Path).ToList();
                var fromSortAndFilter = config.Sort.Select(p => p.PropertyPath).Concat(filterPaths)
                                            .Select(p => p.Split('.').First()).Distinct().ToList();

                foreach (var missing in fromSortAndFilter.Where(p => !columns.Any(x => x.Property == p)).ToList())
                {
                    columns.Add(new SelectItem(missing));
                }

                propsToQuery = (from p in PropertyConfigurations
                                join c in columns on p.Property.Name equals c.Property
                                select p).ToList();
            }

            var columnDependencies = propsToQuery.SelectMany(p => p.DependsOn).Distinct().ToList();

            var dict = new Dictionary<string, List<string>>();

            ResolveDependencies(columnDependencies, dict);

            var sortedDependencies = new List<string>();

            while (dict.Any())
            {
                var items = dict.Where(p => !p.Value.Any()).Select(p => p.Key).ToList();
                if (!items.Any())
                {
                    throw new InvalidOperationException("Circular dependency found!");
                }

                sortedDependencies.AddRange(items);

                foreach (var toRemove in items)
                {
                    dict.Remove(toRemove);
                    foreach (var item in dict)
                    {
                        item.Value.Remove(toRemove);
                    }
                }
            }

            var param = Expression.Parameter(this.SourceType, "p");
            IQueryable baseQuery = CreateBaseQuery(source, sortedDependencies);
            var filter = new FilterItemGroup(config.FilterItems);
            baseQuery = baseQuery.ApplyFilterItem(filter);

            var members = propsToQuery.ToDictionary(p => (MemberInfo)p.PropertyInfo, p => p.MappedExpression.Body.Replace(p.MappedExpression.Parameters.First(), param));

            foreach (var item in PropertyConfigurations.Where(p => !members.ContainsKey(p.PropertyInfo)))
            {
                members.Add(item.PropertyInfo, Expression.Constant(item.Property.DefaultValue, item.PropertyInfo.PropertyType));
            }

            var getInitExpression = GetInitExpression(ResultType, members);
            var selectExpresssion = Expression.Lambda(getInitExpression, param);
            var queryExpression = Expression.Call(
                                        QueryableInfo.Select.MakeGenericMethod(SourceType, ResultType),
                                        baseQuery.Expression,
                                        Expression.Quote(selectExpresssion));

            baseQuery = baseQuery.Provider.CreateQuery(queryExpression);

            if (config.TargetFilterItems.Any())
            {
                var targetFilter = new FilterItemGroup(config.TargetFilterItems);
                baseQuery = baseQuery.ApplyFilterItem(targetFilter);
            }

            if (config.Sort.Any())
            {
                baseQuery = baseQuery.ApplySort(config.Sort);
            }

            return baseQuery;
        }

        private IQueryable CreateBaseQuery(QuerySource source, IEnumerable<string> sortedDependencies)
        {
            var sourceConfig = this.SourceEntityConfigurations.First();
            if (sourceConfig.Name == null)
            {
                return sourceConfig.QueryFactory(source);
            }

            var allConfigs = sortedDependencies
                                .Select(p => this.SourceEntityConfigurations.First(x => x.Name == p))
                                .Select(p => new
                                {
                                    Key = p,
                                    Member = SourceType.GetProperty(p.Name)
                                }).ToList();

            var baseEntity = allConfigs.First();
            var joined = allConfigs.Skip(1).ToList();

            var baseQuery = baseEntity.Key.QueryFactory(source);
            var baseExpression = baseQuery.Expression;

            var param = Expression.Parameter(baseEntity.Key.EntityType, "p");

            Dictionary<MemberInfo, Expression> memberInits = new Dictionary<MemberInfo, Expression>()
            {
                { baseEntity.Member, param }
            };
            foreach (var item in joined)
            {
                memberInits.Add(item.Member, Expression.Constant(null, item.Key.EntityType));
            }

            var body = GetInitExpression(SourceType, memberInits);

            baseExpression = Expression.Call(
                QueryableInfo.Select.MakeGenericMethod(baseEntity.Key.EntityType, SourceType),
                baseExpression,
                Expression.Quote(Expression.Lambda(body, param)));

            for (int i = 0; i < joined.Count; i++)
            {
                var entityType = joined[i].Key.EntityType;
                var joinPairType = typeof(GroupJoinPair<,>).MakeGenericType(SourceType, entityType);
                var localJoin = joined[i].Key.LocalJoinExpression;
                var remoteJoin = joined[i].Key.RemoteJoinExpression;
                var param1 = Expression.Parameter(SourceType, "p1");
                var param2 = Expression.Parameter(typeof(IEnumerable<>).MakeGenericType(entityType), "p2");
                var joinBody = Expression.MemberInit(
                                            Expression.New(joinPairType),
                                            Expression.Bind(joinPairType.GetProperty("Parent"), param1),
                                            Expression.Bind(joinPairType.GetProperty("Children"), param2));

                var test = joined[i].Key.QueryFactory(source);

                ////Expression.Constant(, typeof(IQueryable<>).MakeGenericType(joined[i].Key.EntityType)),

                baseExpression = Expression.Call(
                    QueryableInfo.GroupJoin.MakeGenericMethod(SourceType, entityType, joined[i].Key.JoinKeyType, joinPairType),
                    baseExpression,
                    test.Expression,
                    remoteJoin,
                    localJoin,
                    Expression.Quote(Expression.Lambda(joinBody, param1, param2)));

                param1 = Expression.Parameter(joinPairType, "p1");
                param2 = Expression.Parameter(entityType, "p2");

                memberInits[baseEntity.Member] = Expression.Property(Expression.Property(param1, "Parent"), baseEntity.Key.Name);
                for (int j = 0; j < i; j++)
                {
                    memberInits[joined[j].Member] = Expression.Property(Expression.Property(param1, "Parent"), joined[j].Key.Name);
                }

                memberInits[joined[i].Member] = param2;

                baseExpression = Expression.Call(
                        QueryableInfo.SelectMany.MakeGenericMethod(joinPairType, entityType, SourceType),
                        baseExpression,
                        Expression.Lambda(Expression.Call(EnumerableInfo.DefaultIfEmpty.MakeGenericMethod(entityType), Expression.Property(param1, "Children")), param1),
                        Expression.Lambda(GetInitExpression(SourceType, memberInits), param1, param2));
            }

            return baseQuery.Provider.CreateQuery(baseExpression);
        }

        private Expression GetInitExpression(Type sourceType, IDictionary<MemberInfo, Expression> memberInitis)
        {
            var localInits = memberInitis.ToDictionary(p => p.Key, p => p.Value);

            var ctr = sourceType.GetConstructors().OrderBy(p => p.GetParameters().Count()).First();

            var expressions = new List<Expression>();
            var members = new List<MemberInfo>();

            foreach (var item in ctr.GetParameters())
            {
                var foundMembers = localInits.Where(p => p.Key.Name == item.Name);
                if (foundMembers.Any())
                {
                    var member = foundMembers.First();
                    expressions.Add(member.Value);
                    members.Add(member.Key);
                    localInits.Remove(member.Key);
                }
                else
                {
                    expressions.Add(Expression.Constant(null, item.ParameterType));
                    members.Add(sourceType.GetProperty(item.Name));
                }
            }

            foreach (var item in localInits)
            {
                expressions.Add(item.Value);
                members.Add(item.Key);
            }

            if (!ctr.GetParameters().Any())
            {
                return Expression.MemberInit(
                    Expression.New(ctr),
                    members.Select((p, i) => Expression.Bind(p, expressions[i])));
            }

            return Expression.New(ctr, expressions, members);
        }

        private void ResolveDependencies(IEnumerable<string> columnDependencies, Dictionary<string, List<string>> dict)
        {
            foreach (var item in columnDependencies)
            {
                if (!dict.ContainsKey(item))
                {
                    var source = this.SourceEntityConfigurations.First(p => p.Name == item);
                    dict.Add(item, source.DependsOn.ToList());
                    ResolveDependencies(source.DependsOn, dict);
                }
            }
        }
    }
}