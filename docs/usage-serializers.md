# Serializers

Serializers are used by Cambion to serialize the data being sent over the transport. As with Transports, Cambion can easily be configured to use any serializer, and you can even create your own, but that's also a discussion for another chapter.

## Newtonsoft.JSON

The default serializer that comes preinstalled with Cambion is called `JsonNet` and uses Newtonsoft.Json to serialize the data for the transport. Thisdoes not require any specific configuration, and is used if no other serializer is specified.

```csharp
ICambion cambion = new CambionConfiguration()
    .Serializer.UseJsonNet()
	.Create();
```