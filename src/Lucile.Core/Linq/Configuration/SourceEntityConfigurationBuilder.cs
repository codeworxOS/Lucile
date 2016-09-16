using System;
using System.Linq;
using System.Linq.Expressions;

namespace Lucile.Linq.Configuration
{
    public abstract class SourceEntityConfigurationBuilder
    {
        public Func<QuerySource, IQueryable> QueryFactory { get; set; }

        public Type JoinKeyType { get; set; }

        public LambdaExpression LocalJoinExpression { get; set; }

        public LambdaExpression RemoteJoinExpression { get; set; }
    }
}