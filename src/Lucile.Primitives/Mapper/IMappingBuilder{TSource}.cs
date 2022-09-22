using System;
using System.Linq.Expressions;

namespace Lucile.Mapper
{
    public interface IMappingBuilder<TSource>
    {
        IMappingBuilder<TSource, TTarget> Auto<TTarget>();

        IMappingBuilder<TSource, TTarget> Base<TTarget>();

        IMappingBuilder<TSource, TTarget> Base<TBaseSource, TTarget>();

        IMappingBuilder<TSource, TTarget> To<TTarget>(Expression<Func<TSource, TTarget>> mapping);
    }
}
