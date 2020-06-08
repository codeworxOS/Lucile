namespace Lucile.Data.Metadata.Builder
{
    public interface IMetadataBuilder
    {
        ClrTypeInfo PropertyType { get; }

        bool IsExcluded { get; }

        string Name { get; }

        bool Nullable { get; }
    }
}