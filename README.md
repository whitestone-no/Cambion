# Cambion
A [Succubus](https://github.com/COCPORN/succubus) offspring providing a set of convenience functions for event handling. This not only covers regular asynchronous events, but can also call methods on a subscriber synchronously and get data back.

Uses a local loopback transport by default, but a [NetMQ](https://github.com/zeromq/netmq) transport is also available. Easily extendible to add support for other transports.

## Installation

Cambion is available on [NuGet](https://www.nuget.org/packages/Whitestone.Cambion/) so you can install it in the NuGet Package Manager Console:

```
Install-Package Whitestone.Cambion
```


## Usage

- [The Basics](#the-basics)
  - [Instantiation](#instantiation)
  - [Initialization](#initialization)
- [MEF Compatibility](#mef-compatibility)

### The Basics

#### Instantiation

```csharp
var cambion = new CambionMessageHandler();
```

This instance should preferrably be shared among all usages throughout the code.

#### Initialization

Then initialize it using the default provided transports:

```csharp
messageHandler.Initialize(init => { init.UseLoopback(); });
```

### MEF Compatibility

Cambion also supports [MEF](https://msdn.microsoft.com/en-us/library/dd460648). You can therefore add an `AssemblyCatalog` to your catalogues, then MEF will handle the instantiation and sharing throughout the code:

```csharp
var cambionAssemblyCatalog = new AssemblyCatalog(typeof(ICambion).Assembly)
```

Then you can have MEF import it automatically based on convention:

```csharp
[Import]
private IMessageBus _cambion;
```

Or you can use the MEF container to get an instance:

```csharp
var cambion = yourCompositionContainer.GetExportedValue<ICambion>();
```