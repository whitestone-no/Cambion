# Cambion
Cambion is a lightweight distributed application framework with multiple and pluggable transports and serializers, providing a set of convenience functions for event handling. This not only covers regular asynchronous events, but can also call methods on a subscriber synchronously and get data back.

## Installation

Cambion is available on [NuGet](https://www.nuget.org/packages/Whitestone.Cambion/) so you can install it in the NuGet Package Manager Console:

```
Install-Package Whitestone.Cambion
```


# Usage

- [The Basics](#the-basics)
  - [Instantiation](#instantiation)
  - [Initialization](#initialization)
  - [Subscribing](#subscribing)
    - [Direct subscription](#direct-subscription)
    - [Interface subscription](#interface-subscription)
  - [Consuming](#consuming)
    - [Events](#events)
    - [Synchronized](#synchronized)
- [Transports](#transports)
  - [Loopback](#loopback)
  - [NetMQ](#netmq)
- [Serializers](#serializers)

## The Basics

### Instantiation

```csharp
ICambion cambion = new CambionConfiguration().Create();
```

This creates a configuration object, which you use to create an instance of `ICambion`. This will be initialized with a default Transport and Serializer.
This instance should preferrably be shared among all usages throughout the code.


### Subscribing

There are two ways to say you want to subscribe to an event or synchronizer. The difference between these is that an event is a one-way fire-and-forget message that is intended for multiple recipients, and a synchronized is a two-way message that is intended for one recipient, but can be called from multiple sources.

#### Direct subscription

You can subscribe using the methods found in `ICambion`, which are accessible in your `CambionMessageHandler` object.

```csharp
cambion.AddEventHandler<TEvent>(MyEventCallback);
cambion.AddSynchronizedHandler<TRequest, TResponse>(MySynchronizedCallback);
```

The method signature for the callbacks will then be as follows:

```csharp
private void MyEventCallback(TEvent data) { }
private TResponse MySynchronizedCallback(TRequest data) { }
```

You can also use lambda expressions instead of callback methods:

```csharp
cambion.AddEventHandler<TEvent>(data => { });
cambion.AddSynchronizedHandler<TRequest, TResponse>(data => { return null; });
```

Make sure that your synchronized methods/expressions always return data.

#### Interface subscription

Another way to subscribe is using an interfaces. To use this approach, have the class where you want your subscriptions to implement either `IEventHandler<>` or `ISynchronizedHandler<>`. You can now register all your subscriptions in a single call to `Register()`.

##### Example

```csharp
public class MySubscriptionClass : IEventHandler<TEvent>, ISynchronizedHandler<TRequest, TResponse>
{
    public MySubscriptionClass(ICambion cambion)
    {
        cambion.Register(this);
    }
    
    public void HandleEvent(TEvent data) { }
    
    public TResponse HandleSynchronized(TRequest data)
    {
        return null;
    }
}
```

### Consuming

Once you have all your subscriptions set up you can call the callback methods using the functions provided in `ICambion`.

#### Events

Events are fire-and-forget, and there may be multiple recipients.

Publish an event using the following functionality:

```csharp
cambion.PublishEvent(new TEvent());
```

All the examples regarding events described above in [Subscribing](#subscribing) use the `TEvent` type during subscription, so this call to `PublishEvent()` will cause the callbacks for all those subscriptions to be called.

> Note that you do not need to specify the type as a generic, even though the method signature has it. This will be handled by the compiler.

#### Synchronized

Synchronized are two-way, and there should be only one recipient.

Publish a synchronized using the following functionality:

```csharp
TResponse response = cambion.CallSynchronizedHandler<TRequest, TResponse>(new TRequest());
```

All the examples regarding synchronized described above use the `TRequest` and `TResponse` types during subscription, so this call to `CallSynchronizedHandler` will cause the callbacks for all those subscriptions to be called.

> Note that you are here required to specify the types as generics for the method signature.


#### A note about synchronized messages

Because the distributed transports will use multiple instances of Cambion, a synchronized handler can be set up for the same message signature in multiple instances. Because Cambion sends the synchronized message out to all handlers, there is no way of telling which one will reply, and the reply will be handled by the first instance that is able to send a reply.

Therefore, make sure that no synchronized handler signatures are shared between all your instances.

## Transports

A transport is a layer to transfer messages. Cambion can easily be configured to use any transport, and you can even create your own, but that's a discussion for another chapter.

### Loopback
 
The default transport that comes preinstalled with Cambion is called `Loopback`. This does not require any specific configuration, and is used if no other transport is specified.

```csharp
ICambion cambion = new CambionConfiguration()
    .Transport.UseLoopback()
    .Create();
```

This transport is limited in that Cambion cannot share data with other applications, or even separate instances of Cambion within the same application. For this you need another transport.

### NetMQ

If you have one application that needs to talk to another application, either on the same computer, or even on a different computer, you can't use the loopback transport.

Fortunately Cambion has another transport to cover these use-cases, called `NetMQ`. This uses [NetMQ](https://github.com/zeromq/netmq) to send data between instances of Cambion.

> NetMQ uses two TCP ports, so you need to have these two ports open in your firewall.

The `NetMQ` transport for Cambion is also available on [NuGet](https://www.nuget.org/packages/Whitestone.Cambion.Transport.NetMQ/) so you can install it using the NuGet Package Manager Console:

```
Install-Package Whitestone.Cambion.Transport.NetMQ
```

See the documentation for this transport in its own repo: [Cambion.Transport.NetMQ](https://github.com/whitestone-no/Cambion.Transport.NetMQ)

## Serializers

Serializers are used by Cambion to serialize the data being sent over the transport. As with Transports, Cambion can easily be configured to use any serializer, and you can even crete your own, but that's also a discussion for another chapter.

### Newtonsoft.JSON

The default serializer that comes preinstalled with Cambion is called `JsonNet` and uses Newtonsoft.Json to serialize the data for the transport. Thisdoes not require any specific configuration, and is used if no other serializer is specified.

```csharp
ICambion cambion = new CambionConfiguration()
    .Serializer.UseJsonNet()
	.Create();
```