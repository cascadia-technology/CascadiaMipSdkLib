using System;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Xml;

namespace CascadiaMipSdkLib
{
    internal static class ChannelFactoryBuilder
    {
        public static ChannelFactory<T> BuildChannelFactory<T>(Uri uri, string authType, NetworkCredential nc)
        {
            if (nc == null)
            {
                throw new ArgumentException($"No credential found for {uri}");
            }

            var factory = new ChannelFactory<T>(
                CreateBinding(authType == AuthType.Basic),
                new EndpointAddress(uri, EndpointIdentity.CreateSpnIdentity("host/localhost"))
            );

            if (factory.Credentials == null)
            {
                throw new CommunicationException("Error building WCF channel. ChannelFactory.Credentials is null.");
            }

            switch (authType)
            {
                case AuthType.Basic:
                    factory.Credentials.UserName.UserName = "[BASIC]\\" + nc.UserName;
                    factory.Credentials.UserName.Password = nc.Password;
                    break;
                case AuthType.Negotiate:
                    factory.Credentials.Windows.ClientCredential = nc;
                    break;
                default:
                    throw new ArgumentException($"AuthType '{authType}' is not valid. Expected {AuthType.Basic}, or {AuthType.Negotiate}.");
            }

            return factory;
        }

        public static System.ServiceModel.Channels.Binding CreateBinding(bool isBasic)
        {
            if (isBasic)
            {
                return new BasicHttpBinding
                {
                    ReaderQuotas = XmlDictionaryReaderQuotas.Max,
                    MaxReceivedMessageSize = ChannelSettings.MaxReceivedMessageSize,
                    MaxBufferSize = ChannelSettings.MaxBufferSize,
                    MaxBufferPoolSize = ChannelSettings.MaxBufferPoolSize,
                    HostNameComparisonMode = HostNameComparisonMode.StrongWildcard,
                    MessageEncoding = WSMessageEncoding.Text,
                    TextEncoding = Encoding.UTF8,
                    UseDefaultWebProxy = true,
                    AllowCookies = false,
                    Security =
                    {
                        Mode = BasicHttpSecurityMode.Transport,
                        Transport = {ClientCredentialType = HttpClientCredentialType.Basic}
                    },
                    OpenTimeout = ChannelSettings.Timeouts.OpenTimeout,
                    CloseTimeout = ChannelSettings.Timeouts.CloseTimeout,
                    ReceiveTimeout = ChannelSettings.Timeouts.ReceiveTimeout,
                    SendTimeout = ChannelSettings.Timeouts.SendTimeout
                };
            }

            return new WSHttpBinding
            {
                OpenTimeout = ChannelSettings.Timeouts.OpenTimeout,
                CloseTimeout = ChannelSettings.Timeouts.CloseTimeout,
                ReceiveTimeout = ChannelSettings.Timeouts.ReceiveTimeout,
                SendTimeout = ChannelSettings.Timeouts.SendTimeout,
                ReaderQuotas = XmlDictionaryReaderQuotas.Max,
                MaxReceivedMessageSize = ChannelSettings.MaxReceivedMessageSize,
                MaxBufferPoolSize = ChannelSettings.MaxBufferPoolSize,
                Security =
                {
                    Message =
                    {
                        ClientCredentialType = MessageCredentialType.Windows
                    }
                }
            };
        }
    }
}