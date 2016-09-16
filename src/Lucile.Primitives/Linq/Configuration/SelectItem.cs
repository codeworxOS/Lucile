namespace Lucile.Linq.Configuration
{
    public class SelectItem
    {
        public SelectItem(string property, Aggregate aggregate)
        {
            Property = property;
            Aggregate = aggregate;
        }

        public string Property { get; }

        public Aggregate Aggregate { get; }
    }
}