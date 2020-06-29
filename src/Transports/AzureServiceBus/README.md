# Cambion.Transport.AzureServiceBus
Cambion is a lightweight distributed application framework with multiple and pluggable backends, providing a set of convenience functions for event handling.
If you have one application that needs to talk to another application, either on the same computer, or even on a different computer, you can't use the default
loopback backend provided with Cambion. This transport provider uses [Azure Service Bus](https://azure.microsoft.com/en-us/services/service-bus/) to send data between instances of Cambion.

## Installation
Cambion.Transport.AzureServiceBus is available on [NuGet](https://www.nuget.org/packages/Whitestone.Cambion.Transport.AzureServiceBus/) so you can install it in the NuGet Package Manager Console:
```
Install-Package Whitestone.Cambion.Transport.AzureServiceBus
```

### Documentation
Documentation for the RabbitMQ transport is available on the [Cambion documentation site](https://cambion.readthedocs.io/en/latest)