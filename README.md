# CascadiaMipSdkLib

A helper library to handle some of the more mundane tasks like login, including multi-site login for Milestone Federated Architecture, and construction of WCF proxies for IConfigurationService and IServerCommandService. Using this library may save you time with the boilerplate involved in handling one or more authentication types, enumerating sites in a federated architecture environment, and the MilestoneConnection class is IDisposable so you can use it in a `using` block to cleanly logout and dispose of any lingering Milestone connections.

## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes. See deployment for notes on how to deploy the project on a live system.

### Prerequisites

In order to build this project, you must have the binaries for [Milestone's MIP SDK](https://www.milestonesys.com/community/developer-tools/sdk/) installed to the default location (C:\Program Files\Milestone\MIPSDK\Bin) or update the VideoOS*.dll project references to point to your Milestone MIP SDK binaries.

### Using CascadiaMipSdkLib

Login to a single site as a specific Windows user
```C#
var uri = new Uri("http://xprotect");
var username = "domain\\username";
var password = "password";
using (var connection = new MilestoneConnection(uri, LoginType.Windows, username, password))
{
    connection.Open();
}
```

Login to site and all child sites as the current Windows user
```C#
var uri = new Uri("http://xprotect");
using (var connection = new MilestoneConnection(uri, LoginType.WindowsCurrentUser))
{
    connection.IncludeChildSites = true;
    connection.Open();
}
```

Login to site as a basic user and create WCF proxies for IConfigurationService and IServerCommandService
```C#
var uri = new Uri("http://xprotect");
using (var connection = new MilestoneConnection(uri, LoginType.Basic, "myuser", "password"))
{
    connection.Open();
    
    var configApiResult = connection.CreateChannel<IConfigurationService>();
    if (!configApiResult.Success) throw configApiResult.Exception;
    Console.WriteLine(configApiResult.Channel.GetItem("/").DisplayName);
    
    var scsResult = connection.CreateChannel<IServerCommandService>();
    if (!scsResult.Success) throw scsResult.Exception;
    Console.WriteLine(scsResult.Channel.GetServerVersion());
}
VideoOS.ConfigurationApi.ClientService;
```

Set all WCF proxy client timeout values to 15 minutes
```C#
ChannelSettings.Timeouts.AllTimeouts = TimeSpan.FromMinutes(15);
```

## Authors

* **Joshua Hendricks** - *Initial work* - Senior Principal Support Engineer @ [Milestone Systems](https://www.milestonesys.com) and owner of [Cascadia Technology LLC](https://www.cascadia.tech)
