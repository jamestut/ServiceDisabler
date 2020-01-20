# ServiceDisabler

This simple Windows service will periodically monitor and stop other services in local system. List of services can be configured, and so does the interval.

Update: Now support configuring services to disable. See **Configuration** part for more details.

This service requires .NET framework 4.6.1.

## Installation

Use .NET's `installutil.exe` to install or uninstall this service. By default this service will be installed with `LocalSystem` privileges.

The binary in the releases page are compiled with AnyCPU, therefore both 32 bit and 64 bit version of .NET's `installutil.exe` should work.

Example command to **install** this service:

`C:\Windows\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe C:\path\to\ServiceDisabler.exe`

Example command to **uninstall** this service:

`C:\Windows\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe /u C:\path\to\ServiceDisabler.exe`

Run those commands with administrator privilege.

## Configuration

All configuration data are stored in registry with the following path: ```HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\ServiceDisabler\Config```.

Please note that you may need to create the `Config` key first.

The registry entries are as follows:

- `ServiceNames` (Required)  
Type: `REG_SZ`  
List of services to be monitored and autostopped. Separate each service name with semicolon (`;`). Do not begin or end with semicolon, and do not put any space inside the string.
- `ServiceDisableNames` (Required)  
Type: `REG_SZ`  
List of services to be monitored for their startup status. Services specified here will be automatically disabled for every scan interval. This configuration is independent from `ServiceNames`. Separate each service name with semicolon (`;`). Do not begin or end with semicolon, and do not put any space inside the string.
- `CheckFrequency` (Optional)  
Type: `REG_DWORD`  
Specify the frequency of monitoring, in seconds. If not specified, default to 5 seconds.

## Errors

If the service is misconfigured, service will fail to start and the following errors will be thrown:

Code|Reason
---|---
`0x80070005`|Service is denied to access its configuration registry entry.
`0x80070013`|Wrong data type is specified for configuration entries.
`0x80004005`|Unknown error when opening configuration data.
`0x80070003`|Cannot retreive the status for the specified service.

Once this service is started successfully, this service will not throw any errors even though it may fail modifying the service status.