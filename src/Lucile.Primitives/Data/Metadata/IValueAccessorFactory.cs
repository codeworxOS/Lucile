namespace Lucile.Data.Metadata
{
    public interface IValueAccessorFactory
    {
        IEntityValueAccessor GetAccessor(IEntityMetadata entityMetadata);
    }
}
