namespace Lucile.Data.Metadata.Builder
{
    public interface IMetadataBuilder
    {
        bool IsExcluded { get; set; }

        string Name { get; set; }

        bool Nullable { get; set; }
    }
}