using System;
using System.Collections.Generic;
using System.Net;
using VideoOS.Common.Proxy.Server.WCF;
using VideoOS.ConfigurationApi.ClientService;
using VideoOS.Platform.SDK.Proxy;
using VideoOS.Platform.Util.Security;
using VideoOS.Platform.Util.Svc;

namespace CascadiaMipSdkLib
{
    internal static class ChannelBuilder
    {
        private static readonly Dictionary<Type, string> ServicePaths = new Dictionary<Type, string>();

        static ChannelBuilder()
        {
            /* All Services on Management Server
             * not defined = need to get the wsdl and create classes, see if there's any reliance on DLL's not included with MIP SDK
             *
             * ManagementServerService - not defined
             * RecordingServerService - not defined
             * MediaStorageService - not defined
             * DeviceService - not defined
             * ViewerClientService - not defined
             * FailoverServerService - not defined
             * WebViewerClientService - not defined
             * StateService - not defined
             * SCHDiagnosticsManager - not defined
             * SCHMessageCreatorManager - not defined
             * SCHMessageWatchManager - not defined
             * SCHOperationInfoManager - not defined
             */
            ServicePaths.Add(typeof(IConfigurationService), "/ManagementServer/ConfigurationApiService.svc");
            ServicePaths.Add(typeof(IServerCommandService), "/ManagementServer/ServerCommandService.svc");
            ServicePaths.Add(typeof(IServiceRegistrationService), "/ManagementServer/ServiceRegistrationService.svc");
            ServicePaths.Add(typeof(IServerProxyService), "/ManagementServer/ServerProxyService.svc");
            ServicePaths.Add(typeof(IServerService), "/ManagementServer/ServerService.svc");
            ServicePaths.Add(typeof(ISecurityService), "/ManagementServer/SecurityService.svc");
            
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
            var factory = ChannelFactoryBuilder.BuildChannelFactory<T>(serviceUri, authType, cc.GetCredential(uri, authType));
            
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