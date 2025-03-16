Loopback
--------

The transport that Cambion will use without further configuration is called ``Loopback``, even though it is not a transport in the common sense.
This only bypasses the serialization of data and passes it straight through to the message aggregator.
This does not require any specific configuration, and is used if no other transport or serializer is specified.

::

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddCambion();
    }

This transport is limited in that Cambion cannot share data with other applications, or even separate instances
of Cambion within the same application. For this you need to use one of the other transports available for Cambion.
