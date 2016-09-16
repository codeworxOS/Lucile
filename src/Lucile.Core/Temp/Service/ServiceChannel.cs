using System;
using System.Threading;
using System.Threading.Tasks;
using Codeworx.Core.Service;
using Codeworx.Core.ViewModel;
using Codeworx.ViewModel;

namespace Codeworx.Service
{
    public class ServiceChannel<TService> where TService : class
    {
        public ViewModelBase ViewModel { get; private set; }

        public ServiceChannel(ViewModelBase viewModel)
        {
            this.ViewModel = viewModel;
        }

        public ServiceChannel()
        {

        }

        public Task<TResult> Call<TResult>(Func<TService, Task<TResult>> call, string busyMessage = null)
        {
            return Call<TResult>(call, CancellationToken.None, busyMessage);
        }

        public async Task<TResult> Call<TResult>(Func<TService, Task<TResult>> call, CancellationToken token, string busyMessage = null)
        {
            JobScope scope = null;
            IDisposable disp = null;
            try {
                if (this.ViewModel != null) {
                    scope = this.ViewModel.StartJob(busyMessage);
                }
                token.ThrowIfCancellationRequested();

                var service = await ServiceContext.Current.GetServiceAsync<TService>(token);
                disp = service as IDisposable;

                token.ThrowIfCancellationRequested();
                
                if (service == null) {
                    return default(TResult);
                }
                var result = await call(service);

                token.ThrowIfCancellationRequested();
                return result;
            } finally {
                if (scope != null)
                    scope.Dispose();

                if (disp != null)
                    disp.Dispose();
            }
        }

        public Task Call(Func<TService, Task> call, string busyMessage = null)
        {
            return Call(call, CancellationToken.None, busyMessage);
        }

        public async Task Call(Func<TService, Task> call, CancellationToken token, string busyMessage = null)
        {
            JobScope scope = null;
            IDisposable disp = null;
            try {
                if (this.ViewModel != null) {
                    scope = this.ViewModel.StartJob(busyMessage);
                }
                token.ThrowIfCancellationRequested();

                var service = await ServiceContext.Current.GetServiceAsync<TService>(token);
                disp = service as IDisposable;

                token.ThrowIfCancellationRequested();
                if (service == null) {
                    return;
                }
                await call(service);
            } finally {
                if (scope != null)
                    scope.Dispose();

                if (disp != null)
                    disp.Dispose();
            }
        }
    }
}
