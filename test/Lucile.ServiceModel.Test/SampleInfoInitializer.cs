using System;
using System.ServiceModel.Channels;
using Lucile.ServiceModel.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Tests
{
    public class SampleInfoInitializer : IMessageScopeInitializer
    {
        public IMessageScope Initialize(ref Message message)
        {
            return new SampleInfoScope(message.Headers.To.ToString());
        }

        private class SampleInfoScope : IMessageScope
        {
            public SampleInfoScope(string info)
            {
                Info = info;
            }

            public string Info { get; }

            public void Dispose()
            {
            }

            public void Register(IServiceProvider provider)
            {
                var smpleInfo = provider.GetService<SampleInfo>();
                smpleInfo.Info = Info;
            }
        }
    }
}