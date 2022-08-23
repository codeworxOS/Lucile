using System;
using System.Collections.Generic;

namespace Lucile.Mapper
{
    public interface IMapperFactory
    {
        IMapper<TSource, TTarget> CreateMapper<TSource, TTarget>();

        IMapper CreateMapper(Type sourceType, Type targetType);

        IEnumerable<IMapper> GetMappers(Type sourceType);
    }
}
