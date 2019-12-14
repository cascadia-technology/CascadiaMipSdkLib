using System;
using System.Collections.Generic;
using System.Net;
using VideoOS.Platform;
using VideoOS.Platform.Login;

namespace CascadiaMipSdkLib
{
    public class MilestoneConnection : IDisposable
    {
        private readonly Uri _uri;
        private readonly CredentialCache _cc;
        private readonly LoginType _loginType;

        private LoginSettings _loginSettings;

        public Item MasterSite => EnvironmentManager.Instance.GetSiteItem(_loginSettings.Guid);

        public Item CurrentSite { get; set; }

        public bool IncludeChildSites { get; set; }

        public MipWcfServices Services { get; private set; }

        private string AuthType => _loginType == LoginType.Basic ? "Basic" : "Negotiate";

        public MilestoneConnection(Uri uri, LoginType loginType, NetworkCredential nc)
        {
            _uri = uri;
            _loginType = loginType;
            Services = new MipWcfServices(this);
            switch (loginType)
            {
                case LoginType.Basic:
                case LoginType.Windows:
                    _cc = new CredentialCache {{uri, AuthType, nc}};
                    break;
                case LoginType.WindowsCurrentUser:
                    _cc = new CredentialCache {{uri, AuthType, CredentialCache.DefaultNetworkCredentials}};
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(loginType), loginType, null);
            }
        }

        public void Open()
        {
            VideoOS.Platform.SDK.Environment.Initialize();
            VideoOS.Platform.SDK.Environment.AddServer(_uri, _cc);
            VideoOS.Platform.SDK.Environment.Login(_uri);
            _loginSettings = LoginSettingsCache.GetLoginSettings(_uri.Host, _uri.Port);
            CurrentSite = MasterSite;
            if (!IncludeChildSites) return;

            var stack = new Stack<Item>(MasterSite.GetChildren());
            while (stack.Count > 0)
            {
                var item = stack.Pop();
                AddSite(item.FQID.ServerId.Uri);
                item.GetChildren().ForEach(stack.Push);
            }
        }

        private void AddSite(Uri uri)
        {
            _cc.Add(uri, AuthType, _loginSettings.NetworkCredential);
            VideoOS.Platform.SDK.Environment.AddServer(uri, _cc);
            VideoOS.Platform.SDK.Environment.Login(uri);
        }

        public IEnumerable<Item> GetSites()
        {
            var stack = new Stack<Item>(new []{MasterSite});
            while (stack.Count > 0)
            {
                var item = stack.Pop();
                yield return item;
                item.GetChildren().ForEach(stack.Push);
            }
        }

        public CreateChannelResult<T> CreateChannel<T>(ServerId serverId = null)
        {
            serverId = serverId ?? MasterSite.FQID.ServerId;
            var uri = LoginSettingsCache.GetLoginSettings(serverId).Uri;
            return ChannelBuilder.BuildChannel<T>(uri, AuthType, _cc);
        }

        public string GetCurrentToken(ServerId serverId = null)
        {
            serverId = serverId ?? MasterSite.FQID.ServerId;
            var settings = LoginSettingsCache.GetLoginSettings(serverId);
            return settings.Token;
        }

        public void Dispose()
        {
            foreach (var site in GetSites())
            {
                if (!VideoOS.Platform.SDK.Environment.IsLoggedIn(site.FQID.ServerId.Uri))
                    continue;
                VideoOS.Platform.SDK.Environment.Logout(site.FQID.ServerId.Uri);
                VideoOS.Platform.SDK.Environment.RemoveServer(site.FQID.ServerId.Uri);
            }
        }
    }
}
