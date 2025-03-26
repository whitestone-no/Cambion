.. _refConfigurationReader:

Configuration reader
--------------------

This extension reads ``ITransport`` and ``ISerializer`` settings from the .NET configuration – whether
you’re using an ``appsettings.json`` file, environment variables, user secrets, or similar sources.

Installation
============

The extension is available on `NuGet <https://www.nuget.org/packages/Whitestone.Cambion.Extension.ConfigurationReader/>`_
Install it using the NuGet Package Manager Console:

::

    Install-Package Whitestone.Cambion.Extension.ConfigurationReader

Usage
=====

Add the extension immediately after adding Cambion itself:

::

    Host.CreateDefaultBuilder()
        .ConfigureAppConfiguration((context, builder) =>
        {
            builder.AddJsonFile("appsettings.json");
            builder.AddEnvironmentVariables();
            builder.AddUserSecrets(GetType().Assembly);
        })
        .ConfigureServices((ctx, services) => {
            services.AddCambion()
                .ReadConfiguration(ctx.Configuration);
        });

By default, this will look for a configuration section called ``Cambion``.
You can override this by passing a custom section name into the extension method:

::

    services.AddCambion()
        .ReadConfiguration(ctx.Configuration, "Example");

The configuration section is expected to contain both a ``Transport`` section and a ``Serializer`` section.

.. note:: These sections cannot be renamed.

At a minimum, each subsection must include a string value that specifies the assembly name of the Transport or Serializer to use with Cambion.

Each section must contain at least a string value specifying the assembly name for the Transport or Serializer
to use with Cambion.
In most cases, a Transport requires additional settings. In that scenario, the section should provide the assembly name
along with a nested section for the specific configuration settings.

Example
^^^^^^^

Below is an example of an ``appsettings.json`` file that configures a transport with specific settings
and a serializer without additional settings:

::

    {
        "Cambion": {
            "Transport": {
                "Whitestone.Cambion.Transport.NetMQ": {
                    "PublishAddress": "tcp://localhost:9999",
                    "SubscribeAddress": "tcp://localhost:9998"
                }
            },
            "Serializer": "Whitestone.Cambion.Serializer.MessagePack"
        }
    }

.. note:: See the documentation for each specific transport and serializer for details on required settings and additional examples.

Order of operations
===================

You can combine ``.ReadConfiguration()`` with calls to configure a specific transport and/or serializer:

::

    services.AddCambion()
        .ReadConfiguration(ctx.Configuration)
        .UseNetMqTransport(conf => conf.PublishAddress = "tcp://localhost:8888");

In this example, the configuration initially sets ``PublishAddress`` to ``tcp://localhost:9999``.
The subsequent transport configuration call then overwrites the ``PublishAddress`` to ``tcp://localhost:8888``,
while the ``SubscribeAddress`` remains unchanged at ``tcp://localhost:9998``.

This approach allows you, for instance, to retrieve secret variables from a key vault in your code and inject
them without exposing these values via environment variables.

.. note:: All officially supported transports can also read their configurations independently. See the documentation for each specific transport for further details.

.. note:: If you use another transport than what is specified in the configuration, the configuration will be overwritten and the hardcoded transport will be used.
