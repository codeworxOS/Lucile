using System;
using System.Collections.Generic;
using System.Linq;
using Lucile.Data.Metadata;
using Lucile.Linq.Configuration;

namespace Lucile.Linq
{
    public class QueryModel<TResult> : QueryModel
    {
        private readonly Type _sourceType;

        public QueryModel(Type sourceType, EntityMetadata entity, IEnumerable<SourceEntityConfiguration> sourceEntityConfigs, IEnumerable<PropertyConfiguration> propConfigs)
            : base(entity, sourceEntityConfigs, propConfigs)
        {
            _sourceType = sourceType;
        }

        public override Type ResultType => typeof(TResult);

        public override Type SourceType => _sourceType;

        public new IQueryable<TResult> GetQuery(QuerySource source, QueryConfiguration config)
        {
            return (IQueryable<TResult>)base.GetQuery(source, config);
        }
    }
}