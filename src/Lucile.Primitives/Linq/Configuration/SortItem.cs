namespace Lucile.Linq.Configuration
{
    public class SortItem
    {
        public SortItem(string property, SortDirection sortDirection)
        {
            Property = property;
            SortDirection = sortDirection;
        }

        public string Property { get; }

        public SortDirection SortDirection { get; }
    }
}
