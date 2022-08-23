namespace Lucile.Mapper
{
    public class MapperProxy<TSource, TTarget> : IMapper<TSource, TTarget>
    {
        private readonly IMapper<TSource, TTarget> _target;

        public MapperProxy(IMapperFactory mapperFactory)
        {
            _target = mapperFactory.CreateMapper<TSource, TTarget>();
        }

        public IMappingConfiguration<TSource, TTarget> Configuration => _target.Configuration;

        IMappingConfiguration IMapper.Configuration => ((IMapper)_target).Configuration;

        public TTarget Map(TSource source)
        {
            return _target.Map(source);
        }

        public object Map(object source)
        {
            return _target.Map(source);
        }
    }
}