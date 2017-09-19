namespace Lucile.Linq.Configuration
{
    public class SelectItem
    {
        public SelectItem(string property)
            : this(property, Aggregate.None)
        {
        }

        public SelectItem(string property, Aggregate aggregate)
        {
            Property = property;
            Aggregate = aggregate;
        }

        public Aggregate Aggregate { get; }

        public string Property { get; }
    }
}