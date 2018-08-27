using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;

namespace Lucile.ServiceModel
{
    public class RemoteServiceOptions
    {
        internal RemoteServiceOptions(string baseAddress, long maxMessageSize, ServiceAuthentication authentication, Action<ChannelFactory> onChannelFactoryAction, Func<string, Type, string> addressConvention)
        {
            OnChannelFactoryAction = onChannelFactoryAction;
            Authentication = authentication;
            BaseAddress = baseAddress;
            MaxMessageSize = maxMessageSize;
            AddressConvention = addressConvention ?? new Func<string, Type, string>((ba, contract) => $"{ba.TrimEnd('/')}/{contract.Name.Substring(1)}.svc");
        }

        public Func<string, Type, string> AddressConvention { get; }

        public ServiceAuthentication Authentication { get; }

        public string BaseAddress { get; }

        public long MaxMessageSize { get; }

        public Action<ChannelFactory> OnChannelFactoryAction { get; }

        internal Binding GetBinding<T>()
        {
            Binding binding = null;
            var uri = new Uri(this.BaseAddress);
            if (uri.Scheme == UriScheme.NetTcp)
            {
                NetTcpBinding nettcp = new NetTcpBinding(SecurityMode.Transport) { MaxReceivedMessageSize = this.MaxMessageSize, ReaderQuotas = XmlDictionaryReaderQuotas.Max };

                switch (this.Authentication)
                {
                    case ServiceAuthentication.None:
                        nettcp.Security.Mode = SecurityMode.None;
                        break;

                    case ServiceAuthentication.Windows:
                        nettcp.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
                        break;

                    case ServiceAuthentication.UsernamePassword:
                        nettcp.Security.Mode = SecurityMode.TransportWithMessageCredential;
                        nettcp.Security.Message.ClientCredentialType = MessageCredentialType.UserName;
                        break;

                    case ServiceAuthentication.Certificate:
                        nettcp.Security.Transport.ClientCredentialType = TcpClientCredentialType.Certificate;
                        break;
                }

                binding = nettcp;
            }
            else if (uri.Scheme == UriScheme.Http)
            {
                BasicHttpBinding basic = null;

                switch (this.Authentication)
                {
                    case ServiceAuthentication.None:
                        basic = new BasicHttpBinding(BasicHttpSecurityMode.None);
                        break;

                    case ServiceAuthentication.Windows:
                        basic = new BasicHttpBinding(BasicHttpSecurityMode.TransportCredentialOnly);
                        basic.Security.Transport.ClientCredentialType = HttpClientCredentialType.Windows;
                        break;

                    case ServiceAuthentication.UsernamePassword:
                        throw new NotSupportedException("UserNamePassword authentication is not supported on http base addresses");
                    case ServiceAuthentication.Certificate:
                        basic = new BasicHttpBinding(BasicHttpSecurityMode.TransportCredentialOnly);
                        basic.Security.Transport.ClientCredentialType = HttpClientCredentialType.Certificate;
                        break;
                }

                basic.MaxReceivedMessageSize = this.MaxMessageSize;
                basic.ReaderQuotas = XmlDictionaryReaderQuotas.Max;

                binding = basic;
            }
            else if (uri.Scheme == UriScheme.Https)
            {
                BasicHttpBinding basic = null;

                switch (this.Authentication)
                {
                    case ServiceAuthentication.None:
                        basic = new BasicHttpBinding(BasicHttpSecurityMode.Transport);
                        break;

                    case ServiceAuthentication.Windows:
                        basic = new BasicHttpBinding(BasicHttpSecurityMode.Transport);
                        basic.Security.Transport.ClientCredentialType = HttpClientCredentialType.Windows;
                        break;

                    case ServiceAuthentication.UsernamePassword:
                        throw new NotSupportedException("UserNamePassword authentication is not supported on https base addresses");
                    case ServiceAuthentication.Certificate:
                        basic = new BasicHttpBinding(BasicHttpSecurityMode.Transport);
                        basic.Security.Transport.ClientCredentialType = HttpClientCredentialType.Certificate;
                        break;
                }

                basic.MaxReceivedMessageSize = this.MaxMessageSize;
                basic.ReaderQuotas = XmlDictionaryReaderQuotas.Max;
                binding = basic;
            }

            return binding;
        }

        internal EndpointAddress GetEndpointAddress<T>()
        {
            return new EndpointAddress(AddressConvention(BaseAddress, typeof(T)));
        }
    }
}