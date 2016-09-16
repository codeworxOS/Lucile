using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeworx.Core
{
    public class PropertyValueChangingEventArgs<T> : PropertyValueChangingEventArgs
    {
        public T OldValueTyped
        {
            get
            {
                return (T)OldValue;

            }
        }

        public T NewValueTyped
        {
            get
            {
                return (T)NewValue;
            }
        }

        public PropertyValueChangingEventArgs(string propertyName, T oldValue, T newValue) : base(propertyName, oldValue, newValue) { }
    }
}
