MessagePack
-----------

``MessagePack`` is a more efficient serializer in that it uses less data and is much quicker when serializing/deserializing.
This is therefore a good candidate to use in a production environment.

Installation
============

As with transports, the ``MessagePack`` serializer for Cambion is available on `NuGet <https://www.nuget.org/packages/Whitestone.Cambion.Serializer.MessagePack/>`_ so you can install it using the NuGet Package Manager Console:

::

    Install-Package Whitestone.Cambion.Serializer.MessagePack

Usage
=====

The MessagePack serializer can be set up using an extension method to ``ICambionSerializerBuilder``:

::

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddCambion()
            .UseExampleTransport()
            .WithMessagePackSerializer();
    }

External configuration
======================

If you're using the :ref:`Configuration Reader<refConfigurationReader>` then adding this serializer doesn't require any
additional configuration. You just have to specify the name of the serializer.

Example JSON
^^^^^^^^^^^^

::

    {
        "Cambion": {
            "Serializer": "Whitestone.Cambion.Serializer.MessagePack"
        }
    }
