using System;

namespace Lucile.ViewModel
{
    public class JobScope : IDisposable
    {
        private bool _disposed;
        private object _state;

        public JobScope(ViewModelBase viewModel, string busyMessage = null)
        {
            if (viewModel == null)
            {
                throw new ArgumentNullException(nameof(viewModel));
            }

            this._state = new object();

            this.ViewModel = viewModel;
            this.ViewModel.AddJob(_state, busyMessage);
        }

        ~JobScope()
        {
            Dispose(false);
        }

        public ViewModelBase ViewModel { get; private set; }

        protected virtual bool IsDisposed
        {
            get
            {
                return _disposed;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;

                if (disposing)
                {
                    this.ViewModel.RemoveJob(this._state);
                    this.ViewModel = null;
                    this._state = null;
                }
            }
        }
    }
}