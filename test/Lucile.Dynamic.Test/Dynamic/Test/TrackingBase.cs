using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Lucile.Data.Tracking;

namespace Lucile.Dynamic.Test.Dynamic.Test
{
    [DataContract]
    public abstract class TrackingBase : NotificationObject, ITrackable
    {
        private readonly List<string> _modifiedProperties;

        public TrackingBase()
        {
            _modifiedProperties = new List<string>();
            ModifiedProperties = new ReadOnlyCollection<string>(_modifiedProperties);
        }

        public IEnumerable<string> ModifiedProperties { get; }

        public TrackingState? State { get; set; }

        protected virtual void RegisterModifiedProperty(string propertyName)
        {
            if (!_modifiedProperties.Contains(propertyName))
            {
                _modifiedProperties.Add(propertyName);
            }
        }

        protected virtual void UnregisterModifiedProperty(string propertyName)
        {
            _modifiedProperties.Remove(propertyName);
        }

        #region Protected Methods

        protected override void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.RaisePropertyChanged(propertyName);

            if (this.State.HasValue)
            {
                if (this.State == TrackingState.Unchanged || this.State == TrackingState.Modified)
                {
                    if (!this.ModifiedProperties.Any(p => p.Equals(propertyName)))
                    {
                        this.RegisterModifiedProperty(propertyName);
                    }
                }
                if (this.State.Value == TrackingState.Unchanged)
                {
                    this.State = TrackingState.Modified;
                }
            }
        }

        #endregion Protected Methods

        #region Public Methods

        public void ResetChanges()
        {
            this.ModifiedProperties.ToList().ForEach(p => this.UnregisterModifiedProperty(p));
            this.State = TrackingState.Unchanged;
        }

        #endregion Public Methods
    }
}