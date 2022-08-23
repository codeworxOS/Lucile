namespace Lucile.Mapper
{
    public interface IMapper
    {
        IMappingConfiguration Configuration { get; }

        object Map(object source);
    }
}
