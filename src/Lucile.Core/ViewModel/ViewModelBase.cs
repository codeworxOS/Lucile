using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace Lucile.ViewModel
{
    public class ViewModelBase : NotificationObject, IActivationAware, INavigationAware
    {
        private ConcurrentDictionary<object, string> _busyStates;

        private bool _isActive;

        public ViewModelBase()
        {
            this._busyStates = new ConcurrentDictionary<object, string>();
        }

        public string BusyMessage { get; set; }

        public bool IsActive
        {
            get
            {
                return _isActive;
            }

            private set
            {
                if (_isActive != value)
                {
                    bool handled = false;
                    handled = handled || RaisePropertyChanging(oldValue: _isActive, newValue: value);
                    if (!handled)
                    {
                        _isActive = value;
                        RaisePropertyChanged();
                    }
                }
            }
        }

        public bool IsBusy { get; set; }

        protected bool WasActivated { get; private set; }

        public async Task ActivateAsync(ActivateArgs args)
        {
            if (!WasActivated)
            {
                await OnFirstActivateAsync(args);
                WasActivated = true;
            }

            await OnActivateAsync(args);
            IsActive = true;
        }

        public async Task CloseAsync(CloseArgs args)
        {
            await OnCloseAsync(args);
        }

        public async Task DeactivateAsync(DeactivateArgs args)
        {
            await OnDeactivateAsync(args);
            if (!args.Cancel)
            {
                this.IsActive = false;
            }
        }

        public async Task InitializeAsync()
        {
            await OnInitializeAsync();
        }

        protected internal virtual void AddJob(object state, string busyMessage = null)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            this._busyStates.AddOrUpdate(state, busyMessage, (p, q) => busyMessage);

            SetBusyProperties();
        }

        protected internal virtual void RemoveJob(object state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            string message;

            if (this._busyStates.TryRemove(state, out message))
            {
                SetBusyProperties();
            }
        }

        protected internal JobScope StartJob(string busyMessage = null)
        {
            return new JobScope(this, busyMessage);
        }

#pragma warning disable 1998

        protected virtual async Task OnActivateAsync(ActivateArgs args)
        {
        }

        protected virtual async Task OnCloseAsync(CloseArgs args)
        {
        }

        protected virtual async Task OnDeactivateAsync(DeactivateArgs args)
        {
        }

        protected virtual async Task OnFirstActivateAsync(ActivateArgs args)
        {
        }

        protected virtual async Task OnInitializeAsync()
        {
        }

#pragma warning restore 1998

        private void SetBusyProperties()
        {
            var isBusy = this.IsBusy;
            var message = this.BusyMessage;

            this.IsBusy = this._busyStates.Count > 0;
            this.BusyMessage = this._busyStates.Values.FirstOrDefault();

            if (this.IsBusy != isBusy)
            {
                RaisePropertyChanged(nameof(IsBusy));
            }

            if (this.BusyMessage != message)
            {
                RaisePropertyChanged(nameof(BusyMessage));
            }
        }
    }
}