using System;
using System.Linq.Expressions;

namespace Lucile.Mapper
{
    public interface IMappingBuilder<TSource, TTarget>
    {
        IMappingBuilder<TSource, TNewTarget> Extend<TNewTarget>(Expression<Func<TSource, TTarget>> mapping)
            where TNewTarget : TTarget;

        IMappingBuilder<TSource, TTarget> Partial<TPartial>();

        IMappingConfiguration<TSource, TTarget> ToConfiguration();
    }
}
