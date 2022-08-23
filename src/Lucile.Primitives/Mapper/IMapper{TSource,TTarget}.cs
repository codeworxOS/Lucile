namespace Lucile.Mapper
{
    public interface IMapper<TSource, TTarget> : IMapper
    {
        new IMappingConfiguration<TSource, TTarget> Configuration { get; }

        TTarget Map(TSource source);
    }
}
