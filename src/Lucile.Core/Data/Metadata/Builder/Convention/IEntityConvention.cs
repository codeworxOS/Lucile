namespace Lucile.Data.Metadata.Builder.Convention
{
    public interface IEntityConvention : IModelConvention
    {
        void Apply(EntityMetadataBuilder entity);
    }
}