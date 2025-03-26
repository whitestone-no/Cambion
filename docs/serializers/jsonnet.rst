JSON.NET
--------

This serializer uses Newtonsoft.Json to serialize the data for the transport.
Because this serializes the data into human readable ``JSON`` this is a good candidate to use during development, but is also robust enough
to be used in a production environment.

Installation
============

As with transports, the ``Json.NET`` serializer for Cambion is available on `NuGet <https://www.nuget.org/packages/Whitestone.Cambion.Serializer.JsonNet/>`_ so you can install it using the NuGet Package Manager Console:

::

    Install-Package Whitestone.Cambion.Serializer.JsonNet

Usage
=====

The serializer can be set up using an extension method to ``ICambionSerializerBuilder``:

::

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddCambion()
            .UseExampleTransport()
            .WithJsonNetSerializer();
    }


External configuration
======================

If you're using the :ref:`Configuration Reader<refConfigurationReader>` then adding this serializer doesn't require any
additional configuration. You just have to specify the name of the serializer.

Example JSON
^^^^^^^^^^^^

{
    "Cambion": {
        "Serializer": "Whitestone.Cambion.Serializer.JsonNet"
    }
}