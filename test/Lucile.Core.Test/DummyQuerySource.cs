using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Lucile.Linq;

namespace Tests
{
    internal class DummyQuerySource : QuerySource
    {
        public override IQueryable<TEntity> Query<TEntity>()
        {
            return Enumerable.Empty<TEntity>().AsQueryable();
        }
    }
}