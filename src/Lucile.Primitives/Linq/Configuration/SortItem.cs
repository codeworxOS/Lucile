namespace Lucile.Linq.Configuration
{
    public class SortItem
    {
        public SortItem(string propertyPath, SortDirection sortDirection)
        {
            PropertyPath = propertyPath;
            SortDirection = sortDirection;
        }

        public string PropertyPath { get; }

        public SortDirection SortDirection { get; }
    }
}