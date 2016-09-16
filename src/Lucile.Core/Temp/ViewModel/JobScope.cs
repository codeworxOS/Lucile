using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Codeworx.Core.ViewModel;

namespace Codeworx.ViewModel
{
    public class JobScope : IDisposable
    {
        public ViewModelBase ViewModel { get; private set; }

        private object state;

        public JobScope(ViewModelBase viewModel, string busyMessage = null)
        {
            if (viewModel == null)
                throw new ArgumentNullException("viewModel");

            this.state = new object();

            this.ViewModel = viewModel;
            this.ViewModel.AddJob(state, busyMessage);
        }

        #region IDisposable Members

        private bool _disposed;

        protected virtual bool IsDisposed
        {
            get
            {
                return _disposed;
            }
        }

        ~JobScope()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed) {
                _disposed = true;

                if (disposing) {
                    this.ViewModel.RemoveJob(this.state);
                    this.ViewModel = null;
                    this.state = null;
                }
                // Cleanup native resources here!
            }
        }

        #endregion
    }
}
