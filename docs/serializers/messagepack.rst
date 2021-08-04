MessagePack
-----------

``JsonNet`` is the default serializer that comes preinstalled with Cambion. This can be replaced with this serilizer called MessagePack.
MessagePack is a more efficient serializer in that it uses less data and is much quicker when serializing/deserializing.
This is therefore a good candidate to use in a production environment.

Installation
============

As with transports, the ``MessagePack`` serializer for Cambion is available on `NuGet <https://www.nuget.org/packages/Whitestone.Cambion.Serializer.MessagePack/>`_ so you can install it using the NuGet Package Manager Console:

::

    Install-Package Whitestone.Cambion.Serializer.MessagePack

Usage
=====

The MessagePack serializer can be set up using an extension method to ``ICambionBuilder``:

::

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddCambion()
            .UseMessagePackSerializer();
    }
