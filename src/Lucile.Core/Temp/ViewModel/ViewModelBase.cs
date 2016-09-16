using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Codeworx.ViewModel;

namespace Codeworx.Core.ViewModel
{
    [ViewModelProperty("IsActive", typeof(bool), IsReadOnly = true)]
    public partial class ViewModelBase : NotificationObject, IActivationAware, INavigationAware
    {
        public ViewModelBase()
        {
            this.busyStates = new ConcurrentDictionary<object, string>();
        }

        [Obsolete("This virtual method is called from the ViewModelBase constructor, which violates a major microsoft design pattern. This Method will be removed in future versions of this assembly.", true)]
        protected virtual void OnInitialized() { }

        private ConcurrentDictionary<object, string> busyStates;

        public bool IsBusy { get; set; }

        public string BusyMessage { get; set; }

        protected internal JobScope StartJob(string busyMessage = null)
        {
            return new JobScope(this, busyMessage);
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
                throw new ArgumentNullException("state");

            this.busyStates.AddOrUpdate(state, busyMessage, (p, q) => busyMessage);

            SetBusyProperties();
        }

        private void SetBusyProperties()
        {
            var isBusy = this.IsBusy;
            var message = this.BusyMessage;

            this.IsBusy = this.busyStates.Count > 0;
            this.BusyMessage = this.busyStates.Values.FirstOrDefault();

            if (this.IsBusy != isBusy) {
#if(SILVERLIGHT)
                ViewModelOperations.ExecuteAsync(() => OnPropertyChanged("IsBusy")).InvokeAsync();
#else
                OnPropertyChanged("IsBusy");
#endif
            }
            if (this.BusyMessage != message) {
#if(SILVERLIGHT)
                ViewModelOperations.ExecuteAsync(() => OnPropertyChanged("BusyMessage")).InvokeAsync();
#else
                OnPropertyChanged("BusyMessage");
#endif
            }
        }

        /// <summary>
        /// Removes the job for busy state handling. This Method is threadsave. 
        /// The set operations for the IsBusy and BusyMessage property are syncronized with the ViewModelOperations DefaultSynchronizationContext.
        /// </summary>
        /// <param name="state">The state.</param>
        protected internal virtual void RemoveJob(object state)
        {
            if (state == null)
                throw new ArgumentNullException("state");
            string message;

            if (this.busyStates.TryRemove(state, out message)) {
                SetBusyProperties();
            }
        }

        protected bool WasActivated { get; private set; }

#pragma warning disable 1998
        protected virtual async Task OnActivateAsync(ActivateArgs args) { }
        protected virtual async Task OnFirstActivateAsync(ActivateArgs args) { }
        protected virtual async Task OnDeactivateAsync(DeactivateArgs args) { }

        protected virtual async Task OnInitializeAsync() { }
        protected virtual async Task OnCloseAsync(CloseArgs args) { }
#pragma warning restore 1998

        #region IActivationAware Members
        public async Task ActivateAsync(ActivateArgs args)
        {
            if (!WasActivated) {
                await OnFirstActivateAsync(args);
                WasActivated = true;
            }
            await OnActivateAsync(args);
            IsActive = true;
        }

        public async Task DeactivateAsync(DeactivateArgs args)
        {
            await OnDeactivateAsync(args);
            if (!args.Cancel) {
                this.IsActive = false;
            }
        }
        #endregion

        #region INavigationAware Members
        public async Task InitializeAsync()
        {
            await OnInitializeAsync();
        }
        public async Task CloseAsync(CloseArgs args)
        {
            await OnCloseAsync(args);
        }
        #endregion
    }
}
