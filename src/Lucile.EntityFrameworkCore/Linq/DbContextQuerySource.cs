using System.Linq;
using Lucile.Linq;
using Microsoft.EntityFrameworkCore;

namespace Lucile.EntityFrameworkCore.Linq
{
    public class DbContextQuerySource : QuerySource
    {
        private readonly DbContext _context;

        public DbContextQuerySource(DbContext context)
        {
            _context = context;
        }

        public override IQueryable<TEntity> Query<TEntity>()
        {
            return _context.Set<TEntity>();
        }
    }
}
