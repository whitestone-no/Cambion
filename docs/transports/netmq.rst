NetMQ
-----

When the default Loopback transport doesn't cover your use-cases, Cambion has other transports available. One of these uses `NetMQ <https://github.com/zeromq/netmq>`_ to send data between instances of Cambion.

.. note:: NetMQ uses two TCP ports. These two ports needs to be open in your firewall.

Installation
============

The ``NetMQ`` transport for Cambion is also available on `NuGet <https://www.nuget.org/packages/Whitestone.Cambion.Transport.NetMQ/>`_ so you can install it using the NuGet Package Manager Console:

::

    Install-Package Whitestone.Cambion.Transport.NetMQ

Usage
=====

The NetMQ transport is setup during regular Cambion configuration.
TCP ports ``9998`` and ``9999`` are arbitrary ports, and you can use whichever ports you fancy (that are available).
Remember that the ports must be the same (both numerical, and the same order) for all instances of Cambion that will talk to eachother.

Host
^^^^
In order for Cambion to send and receive data using NetMQ, one of the Cambion instances needs to work as a MessageHost.
The other Cambion instances will then connect to the MessageHost as clients.

The NetMQ transport is set up using an extension method for ``ICambionBuilder``. This extension method takes an
``Action<NetMqConfig>`` as the input parameter.

.. note:: The MessageHost will work as a normal client in addition to being the host without any additional configuration

Initialize the NetMQ backend as normal, but set the optional parameter ``useMessageHost`` parameter in the ``UseNetMQ`` extension to ``true``:

::

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddCambion()
            .UseNetMqTransport(conf =>
            {
                conf.PublishAddress = "tcp://localhost:9999";
                conf.SubscribeAddress = "tcp://localhost:9998";
                conf.UseMessageHost = true;
            });
    }

Clients
^^^^^^^

Clients will use the same configuration as above, but will set ``UseMessageHost`` to ``false`` (or omit it, as it defaults to ``false``)


External configuration
======================

In addition to configuring through ``Action<NetMqConfig>`` you can also pass in an ``Microsoft.Extensions.Configuration.IConfiguration`` object
that has been populated with settings through ``appsettings.json``, environment variables, user secrets, or similar sources.

::

    public void ConfigureServices(HostBuilderContext ctx, IServiceCollection services)
    {
        services.AddCambion()
            .UseNetMqTransport(ctx.Configuration);
    }

This expects the configuration to have been set up according to :ref:`Configuration Reader<refConfigurationReader>`.

Any settings missing in the configuration will be set to the default values for the object type in ``NetMqConfig``.

Any settings defined in the configuration can also be owerwritten through the ``Action<NetMqConfig>``:

::

    public void ConfigureServices(HostBuilderContext ctx, IServiceCollection services)
    {
        services.AddCambion()
            .UseNetMqTransport(
                ctx.Configuration,
                conf => conf.PublishAddress = "tcp://localhost:9999");
    }

As with the Configuration Reader you can also override which settings object to read from, so instead of the default ``Cambion``
override it by passing a new configuration key:

::

    public void ConfigureServices(HostBuilderContext ctx, IServiceCollection services)
    {
        services.AddCambion()
            .UseNetMqTransport(ctx.Configuration, "Example");
    }

.. note:: As with the Configuration Reader you cannot change the "Transport" key.

Example JSON
^^^^^^^^^^^^

::

    {
        "Cambion": {
            "Transport": {
                "Whitestone.Cambion.Transport.NetMQ": {
                    "PublishAddress": "tcp://localhost:9990",
                    "SubscribeAddress": "tcp://localhost:9991",
                    "UseMessageHost": true
                }
            }
        }
    }
