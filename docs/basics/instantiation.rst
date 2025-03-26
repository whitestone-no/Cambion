.. _refInstantiation:

Instantiation
-------------

Cambion is designed to live in a .NET host, be it a generic host or a web host, using Dependency Injection. This means you don't have
to instantiate it yourself.
These hosts will always have a method to configure the Dependency Injection container: ``ConfigureServices(IServiceCollection services)``
Inside this method you simply tell it to use Cambion:

::

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddCambion();
    }

The previous example will initialize Cambion as a hosted service with the loopback fallback.

.. note:: The Cambion instance is a singleton so that the same instance is shared among all usages throughout the code.

Finally, you can now inject Cambion into your code:

::

    public class YourUsageClass
    {
        private readonly ICambion _cambion;

        public YourUsageClass(ICambion cambion)
        {
            _cambion = cambion,
        }
    }

Hosted Service
==============

Cambion implements ``IHostedService`` which has a ``StartAsync()`` and ``StopAsync`` method. These are used to start and stop the ``ITransport``
attached to Cambion so that these can be long running services and will be handled appropriately by .NET.
Seeing as .NET handles the lifetime of Cambion, it will also ensure Cambion is disposed of properly.

Reinitialization
================

Should you have some functionality that verifies the connection to the service bus, and you notice that there's something wrong with the connection,
you can manually reinitialize the service bus Transport that Cambion currently uses.

::

    await cambion.ReinitializeAsync();
	
This will stop the current Transport and start it again.

.. note:: This method is provided as a just-in-case and is normally not needed.