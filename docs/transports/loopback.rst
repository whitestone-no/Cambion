Loopback
--------

The default transport that comes preinstalled with Cambion is called ``Loopback``.
This does not require any specific configuration, and is used if no other transport is specified.
Should you want be verbose about it you can still manually define that it should be used

::

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddCambion()
            .UseLoopbackTransport();
    }

This transport is limited in that Cambion cannot share data with other applications, or even separate instances
of Cambion within the same application. For this you need to use one of the other transports available for Cambion.
