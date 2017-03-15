using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;

namespace Lucile.ServiceModel
{
    public class RemoteServiceOptions
    {
        internal RemoteServiceOptions(string baseAddress, long maxMessageSize)
        {
            this.BaseAddress = baseAddress;
            this.MaxMessageSize = maxMessageSize;
        }

        public string BaseAddress { get; }

        public long MaxMessageSize { get; }

        internal Binding GetBinding<T>()
        {
            Binding binding = null;
            var uri = new Uri(this.BaseAddress);
            if (uri.Scheme == UriScheme.NetTcp)
            {
                binding = new NetTcpBinding(SecurityMode.Transport) { MaxReceivedMessageSize = this.MaxMessageSize, ReaderQuotas = XmlDictionaryReaderQuotas.Max };
            }
            else if (uri.Scheme == UriScheme.Http)
            {
                binding = new BasicHttpBinding(BasicHttpSecurityMode.None) { MaxReceivedMessageSize = this.MaxMessageSize, ReaderQuotas = XmlDictionaryReaderQuotas.Max };
            }
            else if (uri.Scheme == UriScheme.Https)
            {
                binding = new BasicHttpBinding(BasicHttpSecurityMode.Transport) { MaxReceivedMessageSize = this.MaxMessageSize, ReaderQuotas = XmlDictionaryReaderQuotas.Max };
            }

            return binding;
        }

        internal EndpointAddress GetEndpointAddress<T>()
        {
            return new EndpointAddress($"{BaseAddress}/{typeof(T).Name.Substring(1)}.srv");
        }
    }
}