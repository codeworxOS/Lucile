namespace Lucile.Data.Metadata
{
    public class MetadataElement
    {
        internal MetadataElement(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public override string ToString()
        {
            return this.Name ?? base.ToString();
        }
    }
}