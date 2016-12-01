using System.ComponentModel;

namespace Lucile
{
    public class PropertyValueChangingEventArgs : PropertyChangingEventArgs
    {
        public PropertyValueChangingEventArgs(string propertyName, object oldValue, object newValue)
            : base(propertyName)
        {
            this.OldValue = oldValue;
            this.NewValue = newValue;
        }

        public bool Handled { get; set; }

        public object NewValue { get; }

        public object OldValue { get; }
    }
}