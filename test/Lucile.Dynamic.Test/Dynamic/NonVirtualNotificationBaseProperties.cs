using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Lucile.Dynamic.Test.Dynamic
{
    public class NonVirtualNotificationBaseProperties : INotifyPropertyChanged
    {
        private byte[] valByteArrayProperty;

        private int valIntProperty;

        private string valStringProperty;

        public event PropertyChangedEventHandler PropertyChanged;

        public byte[] ByteArrayProperty
        {
            get { return valByteArrayProperty; }
            set
            {
                if (valByteArrayProperty != value)
                {
                    valByteArrayProperty = value;
                    RaisePropertyChanged();
                }
            }
        }

        public int IntProperty
        {
            get { return valIntProperty; }
            set
            {
                if (valIntProperty != value)
                {
                    valIntProperty = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string StringProperty
        {
            get { return valStringProperty; }
            set
            {
                if (valStringProperty != value)
                {
                    valStringProperty = value;
                    RaisePropertyChanged();
                }
            }
        }

        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}