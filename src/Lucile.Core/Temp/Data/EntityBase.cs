using Codeworx.Data.Tracking;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Codeworx.Data
{
    [DataContract(IsReference = true)]
    public class EntityBase : INotifyPropertyChanged, ITrackable
    {
        private object modifiedPropertiesLocker = new object();

        [OnDeserializing]
        private void OnDeserializing(StreamingContext c)
        {
            modifiedPropertiesLocker = new object();
        }

        [DataMember]
        private HashSet<string> modifiedProperties;

        public EntityBase()
        {
            this.modifiedProperties = new HashSet<string>();
        }

        public bool RegisterModifiedProperty(string propertyName)
        {
            lock (modifiedPropertiesLocker) {
                return modifiedProperties.Add(propertyName);
            }
        }

        public bool UnregisterModifiedProperty(string propertyName)
        {
            lock (modifiedPropertiesLocker) {
                return modifiedProperties.Remove(propertyName);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        [DataMember]
        public TrackingState? State
        {
            get;
            set;
        }

        public IEnumerable<string> ModifiedProperties
        {
            get
            {
                lock (modifiedPropertiesLocker) {
                    return modifiedProperties.ToList();
                }
            }
        }
    }
}
