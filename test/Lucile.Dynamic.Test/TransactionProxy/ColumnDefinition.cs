using System.ComponentModel;

namespace Lucile.Dynamic.Test.TransactionProxy
{
    public class ColumnDefinition : IColumnDefinition
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public event PropertyChangingEventHandler PropertyChanging;

        public bool AllowGrouping { get; set; }

        public bool AllowResizing { get; set; }

        public bool AllowSorting { get; set; }

        public bool EditableThroughSettings { get; set; }

        public string FieldName { get; set; }

        public int GroupIndex { get; set; }

        public string HeaderToolTip { get; set; }

        public bool ReadOnly { get; set; }

        public int SortIndex { get; set; }

        public ListSortDirection SortOrder { get; set; }

        public bool Visible { get; set; }

        public int VisibleIndex { get; set; }

        public double Width { get; set; }
    }
}