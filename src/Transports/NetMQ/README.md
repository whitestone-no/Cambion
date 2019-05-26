
# Cambion.Transport.NetMQ
Cambion is a lightweight distributed application framework with multiple and pluggable backends, providing a set of convenience functions for event handling. If you have one application that needs to talk to another application, either on the same computer, or even on a different computer, you can't use the default loopback backend provided with Cambion.
This transport provider uses [NetMQ](https://github.com/zeromq/netmq) to send data between instances of Cambion.
> NetMQ uses two TCP ports, so you need to have these two ports open in your firewall.

## Installation
Cambion.Transport.NetMQ will is available on  [NuGet](https://www.nuget.org/packages/Whitestone.Cambion.Transport.NetMQ/)  so you can install it in the NuGet Package Manager Console:
```
Install-Package Whitestone.Cambion.Transport.NetMQ
```
> IMPORTANT!
> The NetMQ transport is not yet available, but will be once Cambion 1.1 is released

## Usage
The NetMQ transport is setup during regular Cambion configuration.
TCP ports `9998` and `9999` are arbitrary ports, and you can use whichever ports you fancy (that are available). Remember that the ports must be the same (both numerical, and the same order) for all instances of Cambion that will talk to eachother.

### Host
In order for Cambion to send and receive data using NetMQ, one of the Cambion instances needs to work as a MessageHost. The other Cambion instances will then connect to the MessageHost as clients.

> The MessageHost will work as a normal client in addition to being the host without any additional configuration

Initialize the NetMQ backend as normal, but set the optional parameter  `useMessageHost` parameter in the `UseNetMQ` extension to `true`:

```csharp
ICambion cambion = new CambionConfiguration()
    .Transport.UseNetMQ(
        publishAddress: "tcp://localhost:9999",
        subscribeAddress: "tcp://localhost:9998",
        useMessageHost: true)
    .Create();
```
### Clients
Clients will use the same configuration as above, but will set `useMessageHost` to `false` (or omit it, as it defaults to `false`)