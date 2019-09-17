using System;
using System.Collections.Generic;
using System.Net;
using System.ServiceModel;
using VideoOS.Common.Proxy.Server.WCF;
using VideoOS.ConfigurationApi.ClientService;
using VideoOS.Platform.Util.Svc;

namespace CascadiaMipSdkLib
{
    internal static class ChannelBuilder
    {
        private static readonly Dictionary<Type, string> ServicePaths = new Dictionary<Type, string>();

        static ChannelBuilder()
        {
            ServicePaths.Add(typeof(IConfigurationService), "/ManagementServer/ConfigurationApiService.svc");
            ServicePaths.Add(typeof(IServerCommandService), "/ManagementServer/ServerCommandService.svc");
            ServicePaths.Add(typeof(IServiceRegistrationService), "/ManagementServer/ServiceRegistrationService.svc");
        }

        public static CreateChannelResult<T> BuildChannel<T>(Uri uri, string authType, CredentialCache cc)
        {
            if (cc == null) throw new ArgumentException();
            var uriBuilder = new UriBuilder(uri)
            {
                Scheme = authType == AuthType.Basic ? "https" : "http",
                Port = authType == AuthType.Basic ? 443 : uri.Port,
                Path = ServicePaths[typeof(T)]
            };
            var serviceUri = uriBuilder.Uri;
            var factory = ChannelFactoryBuilder.BuildChannelFactory<T>(serviceUri, authType);
            if (factory?.Credentials == null) throw new CommunicationException("Error building WCF channel");

            var nc = cc.GetCredential(uri, authType);
            if (nc == null) throw new ArgumentException($"No credential found for {uri}");
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

            ServicePointManager.ServerCertificateValidationCallback = ChannelSettings.RemoteCertificateValidationCallback;
            try
            {
                var channel = factory.CreateChannel();
                return new CreateChannelResult<T>
                {
                    Channel = channel,
                    Success = true
                };
            }
            catch (Exception e)
            {
                return new CreateChannelResult<T>
                {
                    Exception = e
                };
            }
        }
    }
}