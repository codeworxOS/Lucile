using System.Linq;

namespace Lucile.Linq
{
    public abstract class QuerySource
    {
        public abstract IQueryable<TEntity> Query<TEntity>();
    }
}