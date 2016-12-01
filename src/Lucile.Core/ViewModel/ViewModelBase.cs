using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace Lucile.ViewModel
{
    [ViewModelProperty("IsActive", typeof(bool), IsReadOnly = true)]
    public partial class ViewModelBase : NotificationObject, IActivationAware, INavigationAware
    {
        private ConcurrentDictionary<object, string> _busyStates;

        public ViewModelBase()
        {
            this._busyStates = new ConcurrentDictionary<object, string>();
        }

        public string BusyMessage { get; set; }

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

        /// <summary>
        /// Adds the job for busy state handling. This Method is threadsave.
        /// The set operations for the IsBusy and BusyMessage property are syncronized with the ViewModelOperations DefaultSynchronizationContext.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="busyMessage">The busy message.</param>
        protected internal virtual void AddJob(object state, string busyMessage = null)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            this._busyStates.AddOrUpdate(state, busyMessage, (p, q) => busyMessage);

            SetBusyProperties();
        }

        /// <summary>
        /// Removes the job for busy state handling. This Method is threadsave.
        /// The set operations for the IsBusy and BusyMessage property are syncronized with the ViewModelOperations DefaultSynchronizationContext.
        /// </summary>
        /// <param name="state">The state.</param>
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

        [Obsolete("This virtual method is called from the ViewModelBase constructor, which violates a major microsoft design pattern. This Method will be removed in future versions of this assembly.", true)]
        protected virtual void OnInitialized()
        {
        }

        private void SetBusyProperties()
        {
            var isBusy = this.IsBusy;
            var message = this.BusyMessage;

            this.IsBusy = this._busyStates.Count > 0;
            this.BusyMessage = this._busyStates.Values.FirstOrDefault();

            if (this.IsBusy != isBusy)
            {
                RaisePropertyChanged("IsBusy");
            }

            if (this.BusyMessage != message)
            {
                RaisePropertyChanged("BusyMessage");
            }
        }
    }
}