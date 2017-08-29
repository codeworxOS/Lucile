using System;
using System.Linq;
using System.Linq.Expressions;

namespace Lucile.Linq.Configuration
{
    public abstract class SourceEntityConfigurationBuilder
    {
        public Type JoinKeyType { get; set; }

        public LambdaExpression LocalJoinExpression { get; set; }

        public Func<QuerySource, IQueryable> QueryFactory { get; set; }

        public LambdaExpression RemoteJoinExpression { get; set; }

        public abstract SourceEntityConfiguration ToTarget();
    }
}