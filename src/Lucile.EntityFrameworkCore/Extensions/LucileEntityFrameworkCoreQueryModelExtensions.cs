using System.Linq;
using Lucile.EntityFrameworkCore.Linq;
using Lucile.Linq;

namespace Microsoft.EntityFrameworkCore
{
    public static class LucileEntityFrameworkCoreQueryModelExtensions
    {
        public static IQueryable<TEntity> FromQueryModel<TEntity>(this DbContext context, QueryModel<TEntity> model, QueryConfiguration configuration = null)
        {
            return model.GetQuery(new DbContextQuerySource(context), configuration ?? new QueryConfiguration());
        }

        public static IQueryable FromQueryModel(this DbContext context, QueryModel model, QueryConfiguration configuration = null)
        {
            return model.GetQuery(new DbContextQuerySource(context), configuration ?? new QueryConfiguration());
        }
    }
}
