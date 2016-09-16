using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Codeworx.Core
{
    public class PropertyValueChangingEventArgs : PropertyChangingEventArgs
    {
        public object OldValue { get; private set; }

        public object NewValue { get; private set; }

        public bool Handled { get; set; }

        public PropertyValueChangingEventArgs(string propertyName, object oldValue, object newValue)
            : base(propertyName)
        {
            this.OldValue = oldValue;
            this.NewValue = newValue;
        }
    }
}
