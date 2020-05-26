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

