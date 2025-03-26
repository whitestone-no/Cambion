Azure Service Bus
-----------------

Another of the transports available for Cambion makes it able to use `Azure Service Bus <https://azure.microsoft.com/en-us/services/service-bus/>`_ to
send data between instances of Cambion. Cambion uses a pub/sub pattern which requires a dedicated Topic with Subscriptions. Azure Service Bus setup of
these, as well as how to set up Azure authentication is not covered by this documentation as Microsoft has documented this on their own website.

Installation
============

As with other transports, the ``Azure Service Bus`` transport for Cambion is also available on `NuGet <https://www.nuget.org/packages/Whitestone.Cambion.Transport.AzureServiceBus/>`_
so you can install it using the NuGet Package Manager Console:

::

    Install-Package Whitestone.Cambion.Transport.AzureServiceBus

Usage
=====

Seeing as Cambion uses a pub/sub pattern for its inner workings, you will need an existing Azure Service Bus that supports Topics and Subscriptions.
Setting up this in Azure is well `documented <https://docs.microsoft.com/en-us/azure/service-bus-messaging/>`_ by Microsoft and is not covered in this documentation.

The Azure Service Bus transport is set up using an extension method for ``ICambionBuilder``. This extension method takes an
``Action<AzureServiceBusConfig>`` as the input parameter. The most basic values for this configuration are also minimum required values:

::

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddCambion()
            .UseAzureServiceBusTransport(conf =>
            {
                conf.Endpoint = "service-bus-namespace.servicebus.windows.net";
                conf.Topic.Name = "cambion";
                conf.Subscription.Name = "cambion-sub-1";
            });
    }

Authentication
==============

Microsoft recommends using Azure AD for authentication towards their services. Because of this, the Azure Service Bus transport for Cambion does not support
Shared Access Signatures (SAS).

The default authentication used by this transport is Azure Managed Identity. You can also use Azure Active Directory App Registrations (Client Secret).

When the ``AzureServiceBusConfig.Authentication`` configuration setting is ``null`` or any of its sub settings are ``null`` or empty (as it is by default) Cambion
will use Managed Identity to connect to the Azure Service Bus. If you want to use an App Registration instead, fill in appropriate values for all the sub settings.
	
Additional Settings
===================

The ``AzureServiceBusConfig`` object has even more configuration options that could be useful. The following table describes all the configuration options:

+--------------------+----------------+----------+------------------------------------------+--------------------+
| Group              | Parameter name | Required | Description                              | Default value      |
+====================+================+==========+==========================================+====================+
|                    | Endpoint       | **Yes**  | The full URI to your Azure Service Bus   | *null*             |
+--------------------+----------------+----------+------------------------------------------+--------------------+
| **Topic**          | Name           | **Yes**  | The name of the topic                    | *null*             |
|                    +----------------+----------+------------------------------------------+--------------------+
|                    | AutoCreate     | No       | Automatically create the topic if it     | false              |
|                    |                |          | does not already exist                   |                    |
|                    +----------------+----------+------------------------------------------+--------------------+
|                    | AutoDelete     | No       | Automatically deletes the topic when     | false              |
|                    |                |          | application exits and the topic          |                    |
|                    |                |          | doesn't have any connected subscribers   |                    |
|                    +----------------+----------+------------------------------------------+--------------------+
|                    | Details        | No       | Microsoft's own class for describing a   | See below          |
|                    |                |          | topic. Optionally used together with     |                    |
|                    |                |          | AutoCreate for more fine grained         |                    |
|                    |                |          | control of topic creation options        |                    |
+--------------------+----------------+----------+------------------------------------------+--------------------+
| **Subscription**   | Name           | **Yes**  | The name of the subscription             | *null*             |
|                    +----------------+----------+------------------------------------------+--------------------+
|                    | AutoCreate     | No       | Automatically create the subscription    | false              |
|                    |                |          | if it does not already exist             |                    |
|                    +----------------+----------+------------------------------------------+--------------------+
|                    | AutoDelete     | No       | Automatically deletes the subscription   | false              |
|                    |                |          | when application exits                   |                    |
|                    +----------------+----------+------------------------------------------+--------------------+
|                    | Details        | No       | Microsoft's own class for describing a   | See below          |
|                    |                |          | subscription. Optionally used together   |                    |
|                    |                |          | with AutoCreate for more fine grained    |                    |
|                    |                |          | control of subscription creation options |                    |
+--------------------+----------------+----------+------------------------------------------+--------------------+
| **Authentication** | TenantId       | No       | The Azure AD Tenant the Azure Service    | *null*             |
|                    |                |          | bus is connected to                      |                    |
|                    +----------------+----------+------------------------------------------+--------------------+
|                    | ClientId       | No       | The App Registration Client ID           | *null*             |
|                    +----------------+----------+------------------------------------------+--------------------+
|                    | ClientSecret   | No       | The App Registration Client Secret       | *null*             |
+--------------------+----------------+----------+------------------------------------------+--------------------+

The Details Property
^^^^^^^^^^^^^^^^^^^^

The ``Details`` property of the ``AzureServiceBusConfig`` is a Microsoft class with options used for creating a topic or a subscription.
Please refer to Microsoft's documentation (`CreateTopicOptions <https://learn.microsoft.com/en-us/dotnet/api/azure.messaging.servicebus.administration.createtopicoptions?view=azure-dotnet>`_
/ `CreateSubscriptionOptions <https://learn.microsoft.com/en-us/dotnet/api/azure.messaging.servicebus.administration.createsubscriptionoptions>`_) for
the meaning of each property in these classes.

.. note:: The ``Name`` property on the ``TopicDescription`` / ``SubscriptionDescription`` will *always* be overwritten by the transport with the value from the ``AzureServiceBusConfig.Topic.Name`` / `AzureServiceBusConfig.Topic.Name`` properties, so any values you set here will be ignored!

External configuration
======================

In addition to configuring through ``Action<AzureServiceBusConfig>`` you can also pass in an ``Microsoft.Extensions.Configuration.IConfiguration`` object
that has been populated with settings through ``appsettings.json``, environment variables, user secrets, or similar sources.

::

    public void ConfigureServices(HostBuilderContext ctx, IServiceCollection services)
    {
        services.AddCambion()
            .UseAzureServiceBusTransport(ctx.Configuration);
    }

This expects the configuration to have been set up according to :ref:`Configuration Reader<refConfigurationReader>`.

Any settings missing in the configuration will be set to the default values for the object type in ``AzureServiceBusConfig``.

Any settings defined in the configuration can also be owerwritten through the ``Action<AzureServiceBusConfig>``:

::

    public void ConfigureServices(HostBuilderContext ctx, IServiceCollection services)
    {
        services.AddCambion()
            .UseAzureServiceBusTransport(
                ctx.Configuration,
                conf => conf.Endpoint = "service-bus-namespace.servicebus.windows.net");
    }

As with the Configuration Reader you can also override which settings object to read from, so instead of the default ``Cambion``
override it by passing a new configuration key:

::

    public void ConfigureServices(HostBuilderContext ctx, IServiceCollection services)
    {
        services.AddCambion()
            .UseAzureServiceBusTransport(ctx.Configuration, "Example");
    }

.. note:: As with the Configuration Reader you cannot change the "Transport" key.

Example JSON
^^^^^^^^^^^^

{
    "Cambion": {
        "Transport": {
            "Whitestone.Cambion.Transport.AzureServiceBus": {
                "Endpoint": "service-bus-namespace.servicebus.windows.net",
                "Topic": {
                    "Name": "cambion",
                    "AutoCreate": true,
                    "AutoDelete": true
                },
                "Subscription": {
                    "Name": "cambion-sub-1",
                    "AutoCreate": true,
                    "AutoDelete": true
                },
                "Autentication": {
                    "TenantId": "YourTenant",
                    "ClientId": "YourClient",
                    "ClientSecret": "YourSecret"
                }
            }
        }
    }
}