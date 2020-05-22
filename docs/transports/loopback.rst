Loopback
--------

The default transport that comes preinstalled with Cambion is called ``Loopback``.
This does not require any specific configuration, and is used if no other transport is specified.

::

    ICambion cambion = new CambionConfiguration()
        .Transport.UseLoopback()
        .Create();

This transport is limited in that Cambion cannot share data with other applications, or even separate instances
of Cambion within the same application. For this you need to use one of the other transports available for Cambion.
