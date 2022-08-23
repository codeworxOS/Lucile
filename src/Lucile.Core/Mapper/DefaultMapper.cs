namespace Lucile.Mapper
{
    public class DefaultMapper<TSource, TTarget> : IMapper<TSource, TTarget>
    {
        private readonly IMappingConfiguration<TSource, TTarget> _configuration;

        public DefaultMapper(IMappingConfiguration<TSource, TTarget> configuration)
        {
            _configuration = configuration;
        }

        public IMappingConfiguration<TSource, TTarget> Configuration => _configuration;

        IMappingConfiguration IMapper.Configuration => Configuration;

        public TTarget Map(TSource source)
        {
            return _configuration.Convert(source);
        }

        public object Map(object source)
        {
            return _configuration.Convert(source);
        }
    }
}