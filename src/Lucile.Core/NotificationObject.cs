using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Lucile
{
    [DataContract(IsReference = true)]
    public class NotificationObject : INotifyPropertyChanging, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public event PropertyChangingEventHandler PropertyChanging;

        protected virtual void RaisePropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual bool RaisePropertyChanging<T>([CallerMemberName] string propertyName = null, T oldValue = default(T), T newValue = default(T))
        {
            if (this.PropertyChanging != null)
            {
                var eventArgs = new PropertyValueChangingEventArgs<T>(propertyName, oldValue, newValue);
                this.PropertyChanging(this, eventArgs);
                return eventArgs.Handled;
            }

            return false;
        }
    }
}