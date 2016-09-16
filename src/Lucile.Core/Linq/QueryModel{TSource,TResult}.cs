using System;
using System.Collections.Generic;
using System.Linq;
using Lucile.Data.Metadata;
using Lucile.Linq.Configuration;

namespace Lucile.Linq
{
    public class QueryModel<TSource, TResult> : QueryModel
    {
        public QueryModel(EntityMetadata entity, IEnumerable<SourceEntityConfiguration> sourceEntityConfigs, IEnumerable<PropertyConfiguration> propConfigs)
            : base(entity, sourceEntityConfigs, propConfigs)
        {
        }

        public override Type ResultType
        {
            get
            {
                return typeof(TResult);
            }
        }

        public override Type SourceType
        {
            get
            {
                return typeof(TSource);
            }
        }

        public new IQueryable<TResult> GetQuery(QuerySource source, QueryConfiguration config)
        {
            return (IQueryable<TResult>)base.GetQuery(source, config);
        }
    }
}