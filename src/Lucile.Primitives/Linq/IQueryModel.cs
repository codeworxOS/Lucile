using System;
using System.Linq;

namespace Lucile.Linq
{
    public interface IQueryModel
    {
        Type ResultType { get; }

        IQueryable GetQuery(QuerySource source, QueryConfiguration config);
    }
}