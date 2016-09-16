using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Lucile.Linq.Configuration
{
    public class SourceEntityConfiguration
    {
        private static readonly MethodInfo _queryMethodInfo;

        static SourceEntityConfiguration()
        {
            Expression<Func<QuerySource, IQueryable>> tmp = p => p.Query<object>();
            _queryMethodInfo = ((MethodCallExpression)tmp.Body).Method.GetGenericMethodDefinition();
        }

        public SourceEntityConfiguration(string name, Type entityType, Func<QuerySource, IQueryable> queryFactory, Type joinKeyType, LambdaExpression localJoinExpression, LambdaExpression remoteJoinExpression)
        {
            Name = name;
            EntityType = entityType;
            if (queryFactory == null)
            {
                var param = Expression.Parameter(typeof(QuerySource));
                QueryFactory = Expression.Lambda<Func<QuerySource, IQueryable>>(Expression.Call(param, _queryMethodInfo.MakeGenericMethod(entityType)), param).Compile();
            }
            else
            {
                QueryFactory = queryFactory;
            }

            JoinKeyType = joinKeyType;
            LocalJoinExpression = localJoinExpression;
            RemoteJoinExpression = remoteJoinExpression;

            IEnumerable<string> dependencies = Enumerable.Empty<string>();

            if (remoteJoinExpression != null)
            {
                var sourceParam = remoteJoinExpression.Parameters.First();

                dependencies = remoteJoinExpression.Find<MemberExpression>(p => p.Expression == sourceParam)
                                    .Select(p => p.Member.Name);
            }

            DependsOn = ImmutableHashSet.Create<string>(dependencies.ToArray());
            IsRootQuery = DependsOn.Count == 0;
        }

        public ImmutableHashSet<string> DependsOn { get; }

        public Type EntityType { get; }

        public bool IsRootQuery { get; }

        public Type JoinKeyType { get; }

        public LambdaExpression LocalJoinExpression { get; }

        public string Name { get; }

        public Func<QuerySource, IQueryable> QueryFactory { get; }

        public LambdaExpression RemoteJoinExpression { get; }
    }
}