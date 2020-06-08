using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Lucile.AspNetCore.ClientSettings
{
    public class OptionSettingsService<TOption> : IClientSettingsService, IDisposable
    {
        private readonly IDisposable _subscription;
        private bool _disposedValue = false;
        private TOption _option;

        public OptionSettingsService(IOptionsMonitor<TOption> monitor)
        {
            _subscription = monitor.OnChange(p => _option = p);
            _option = monitor.CurrentValue;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public async Task WriteSettingsAsync(Stream response)
        {
            await JsonSerializer.SerializeAsync(response, _option);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _subscription.Dispose();
                }

                _disposedValue = true;
            }
        }
    }
}
