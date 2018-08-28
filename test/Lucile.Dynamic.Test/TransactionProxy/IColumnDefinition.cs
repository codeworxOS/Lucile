using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Lucile.Dynamic.Test.TransactionProxy
{
    public interface IColumnDefinition : INotifyPropertyChanged, INotifyPropertyChanging
    {
        [DataMember]
        bool AllowGrouping { get; set; }

        [DataMember]
        bool AllowResizing { get; set; }

        [DataMember]
        bool AllowSorting { get; set; }

        [IgnoreDataMember]
        bool EditableThroughSettings { get; set; }

        [DataMember]
        string FieldName { get; set; }

        [DataMember]
        int GroupIndex { get; set; }

        [DataMember]
        string HeaderToolTip { get; set; }

        [DataMember]
        bool ReadOnly { get; set; }

        [DataMember]
        int SortIndex { get; set; }

        [DataMember]
        ListSortDirection SortOrder { get; set; }

        [DataMember]
        bool Visible { get; set; }

        [DataMember]
        int VisibleIndex { get; set; }

        [DataMember]
        double Width { get; set; }
    }
}