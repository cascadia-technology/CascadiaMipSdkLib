using System;
using VideoOS.Common.Proxy.Server.WCF;
using VideoOS.ConfigurationApi.ClientService;
using VideoOS.Platform;
using VideoOS.Platform.SDK.Proxy.RecorderServices;
using VideoOS.Platform.SDK.Proxy.Status2;
using VideoOS.Platform.Util.Svc;

namespace CascadiaMipSdkLib
{
    public class MipWcfServices
    {
        private readonly MilestoneConnection _connection;

        public IConfigurationService GetConfigurationService(ServerId serverId = null)
        {
            serverId = serverId ?? _connection.CurrentSite.FQID.ServerId;
            var result = _connection.CreateChannel<IConfigurationService>(serverId);
            if (!result.Success) throw result.Exception;
            return result.Channel;
        }

        public IServerCommandService GetServerCommandService(ServerId serverId = null)
        {
            serverId = serverId ?? _connection.CurrentSite.FQID.ServerId;
            var result = _connection.CreateChannel<IServerCommandService>(serverId);
            if (!result.Success) throw result.Exception;
            return result.Channel;
        }

        public RecorderCommandService GetRecorderCommandService(string host, int port)
        {
            var uri = new Uri($"http://{host}:{port}/RecorderCommandService/RecorderCommandService.asmx");
            return new RecorderCommandService { Url = uri.ToString() };
        }
        
        public RecorderStatusService2 GetRecorderStatusService2(string host, int port)
        {
            var uri = new Uri($"http://{host}:{port}/RecorderStatusService/RecorderStatusService2.asmx");
            return new RecorderStatusService2(uri);
        }

        public IServiceRegistrationService GetServiceRegistrationService(ServerId serverId = null)
        {
            serverId = serverId ?? _connection.CurrentSite.FQID.ServerId;
            var result = _connection.CreateChannel<IServiceRegistrationService>(serverId);
            if (!result.Success) throw result.Exception;
            return result.Channel;
        }

        public MipWcfServices(MilestoneConnection connection)
        {
            _connection = connection;
        }
    }
}