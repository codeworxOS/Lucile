using System;

namespace Lucile.Mapper
{
    public interface IMappingRegistrationBuilder<TSource>
    {
        IMappingRegistrationBuilder<TSource> Configure<TTarget>(Func<IMappingBuilder<TSource>, IMappingBuilder<TSource, TTarget>> builder);
    }
}
