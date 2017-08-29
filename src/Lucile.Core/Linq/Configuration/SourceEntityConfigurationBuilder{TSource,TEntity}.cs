using System;
using System.Linq;
using System.Linq.Expressions;

namespace Lucile.Linq.Configuration
{
    public class SourceEntityConfigurationBuilder<TSource, TEntity> : SourceEntityConfigurationBuilder
    {
        private readonly object _key;

        public SourceEntityConfigurationBuilder(object key)
        {
            _key = key;
        }

        public SourceEntityConfigurationBuilder<TSource, TEntity> Join<TKey>(Expression<Func<TEntity, TKey>> keySelector, Expression<Func<TSource, TKey>> join)
        {
            JoinKeyType = typeof(TKey);
            LocalJoinExpression = keySelector;
            RemoteJoinExpression = join;

            return this;
        }

        public SourceEntityConfigurationBuilder<TSource, TEntity> Query(Func<QuerySource, IQueryable<TEntity>> queryExpression)
        {
            QueryFactory = queryExpression;
            return this;
        }

        public override SourceEntityConfiguration ToTarget()
        {
            return new SourceEntityConfiguration(this._key as string, typeof(TEntity), this.QueryFactory, this.JoinKeyType, this.LocalJoinExpression, this.RemoteJoinExpression);
        }
    }
}