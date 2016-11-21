using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Lucile.Linq;
using Lucile.Reflection;

namespace Tests
{
    internal class DummyQuerySource : QuerySource
    {
        public readonly ConcurrentDictionary<object, IEnumerable<object>> _data;

        public DummyQuerySource()
        {
            _data = new ConcurrentDictionary<object, IEnumerable<object>>();
        }

        public override IQueryable<TEntity> Query<TEntity>()
        {
            IEnumerable<object> data;
            IEnumerable<TEntity> result;

            if (_data.TryGetValue(TypeKey<TEntity>.Key, out data))
            {
                result = (IEnumerable<TEntity>)data;
            }
            else
            {
                result = Enumerable.Empty<TEntity>();
            }

            return result.AsQueryable();
        }

        public void RegisterData<TEntity>(IEnumerable<TEntity> data)
            where TEntity : class
        {
            _data.AddOrUpdate(TypeKey<TEntity>.Key, data, (p, q) => data);
        }
    }
}