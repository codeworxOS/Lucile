using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;

namespace Lucile.ServiceModel.Test
{
    public class DuplexServiceProviderService : IDuplexServiceProviderService
    {
        private readonly IDuplexCallback _callback;

        public DuplexServiceProviderService(IDuplexCallback callback)
        {
            _callback = callback;
        }

        public Task StartImportAsync()
        {
            var t = new Thread(new ThreadStart(() =>
            {
                Thread.Sleep(10);
                _callback.OnProgress(0.1m);
                _callback.OnProgress(0.2m);
                _callback.OnProgress(0.3m);
                _callback.OnProgress(0.4m);
                _callback.OnProgress(0.5m);
                _callback.OnProgress(0.6m);
                _callback.OnProgress(0.7m);
                _callback.OnProgress(0.8m);
                _callback.OnProgress(0.9m);
                _callback.OnProgress(1.0m);
                _callback.OnFinished();
            }));

            t.Start();

            return Task.CompletedTask;
        }
    }
}