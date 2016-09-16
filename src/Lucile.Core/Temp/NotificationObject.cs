using System.ComponentModel;
#if(!NET4 && !SILVERLIGHT)
using System.Runtime.CompilerServices;
#endif
using System.Runtime.Serialization;

namespace Codeworx.Core
{
	[DataContract(IsReference=true)]
    public class NotificationObject : INotifyPropertyChanging, INotifyPropertyChanged
    {
#if(!NET4 && !SILVERLIGHT)
        protected virtual bool OnPropertyChanging<T>([CallerMemberName] string propertyName = null, T oldValue = default(T), T newValue = default(T))
#else
        protected virtual bool OnPropertyChanging<T>(string propertyName, T oldValue, T newValue)
#endif
        {
            if (this.PropertyChanging != null)
            {
                var eventArgs = new PropertyValueChangingEventArgs<T>(propertyName, oldValue, newValue);
                this.PropertyChanging(this, eventArgs);
                return eventArgs.Handled;
            }
            return false;
        }

#if(!NET4 && !SILVERLIGHT)
        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
#else
        protected virtual void OnPropertyChanged(string propertyName)
#endif
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangingEventHandler PropertyChanging;

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
