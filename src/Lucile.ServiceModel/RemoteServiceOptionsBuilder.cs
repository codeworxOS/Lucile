using System;
using System.ServiceModel;

namespace Lucile.ServiceModel
{
    public class RemoteServiceOptionsBuilder
    {
        public RemoteServiceOptionsBuilder()
        {
            this.MaxMessageSize = 1024 * 64;
        }

        public Func<string, Type, string> AddressConvention { get; set; }

        public ServiceAuthentication Authentication { get; set; }

        public string BaseAddress { get; set; }

        public long MaxMessageSize { get; set; }

        public Action<IServiceProvider, ChannelFactory> OnChannelFactoryAction { get; set; }

        public RemoteServiceOptionsBuilder Addressing(Func<string, Type, string> convention)
        {
            AddressConvention = convention;
            return this;
        }

        public RemoteServiceOptionsBuilder Base(string address)
        {
            BaseAddress = address;
            return this;
        }

        public RemoteServiceOptionsBuilder Credentials(ServiceAuthentication auth)
        {
            this.Authentication = auth;

            return this;
        }

        public RemoteServiceOptionsBuilder OnChannelFactory(Action<ChannelFactory> action)
        {
            OnChannelFactoryAction = (p, q) => action(q);
            return this;
        }

        public RemoteServiceOptionsBuilder OnChannelFactory(Action<IServiceProvider, ChannelFactory> action)
        {
            OnChannelFactoryAction = action;
            return this;
        }

        public RemoteServiceOptionsBuilder Size(long maxMessageSize)
        {
            MaxMessageSize = maxMessageSize;
            return this;
        }

        public RemoteServiceOptions ToOptions()
        {
            return new RemoteServiceOptions(this.BaseAddress, this.MaxMessageSize, this.Authentication, this.OnChannelFactoryAction, this.AddressConvention);
        }
    }
}