using System;
using System.Linq.Expressions;

namespace Lucile.Mapper
{
    public interface IMappingBuilder<TSource, TTarget>
    {
        IMappingBuilder<TSource, TNewTarget> Extend<TNewTarget>(Expression<Func<TSource, TNewTarget>> mapping)
            where TNewTarget : TTarget;

        IMappingBuilder<TSource, TTarget> Partial<TPartial>();

        IMappingBuilder<TSource, TTarget> Partial<TPartialSource, TPartial>();

        IMappingConfiguration<TSource, TTarget> ToConfiguration();
    }
}
