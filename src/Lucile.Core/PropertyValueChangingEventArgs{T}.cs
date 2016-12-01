namespace Lucile
{
    public class PropertyValueChangingEventArgs<T> : PropertyValueChangingEventArgs
    {
        public PropertyValueChangingEventArgs(string propertyName, T oldValue, T newValue)
            : base(propertyName, oldValue, newValue)
        {
        }

        public new T NewValue
        {
            get
            {
                return (T)base.NewValue;
            }
        }

        public new T OldValue
        {
            get
            {
                return (T)base.OldValue;
            }
        }
    }
}